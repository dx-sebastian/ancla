import * as cp from 'child_process';
import type { DevPortCatalog } from './devPortCatalog';

export interface PortProcessEntry {
    protocol: string;
    localAddress: string;
    port: number;
    state: string;
    processId: number;
    processName: string;
    executablePath: string;
    isProtected: boolean;
    protectionReason: string;
    devProfileKey: string;
    devProfileLabel: string;
    safetyLabel: string;
    isDevelopmentPort: boolean;
}

interface ProcessInfo {
    name: string;
    executablePath: string;
    sessionId: number;
}

interface NetstatRow {
    protocol: string;
    localAddress: string;
    port: number;
    state: string;
    processId: number;
}

const CRITICAL_PROCESS_NAMES = new Set([
    'csrss', 'dwm', 'fontdrvhost', 'idle', 'lsass',
    'services', 'smss', 'spoolsv', 'svchost', 'system',
    'wininit', 'winlogon'
]);

const WINDOWS_DIR = (process.env['WINDIR'] ?? 'C:\\Windows').toLowerCase();

function runCommand(command: string): string {
    return cp.execSync(command, {
        encoding: 'utf8',
        windowsHide: true,
        timeout: 12000
    });
}

// ---------------------------------------------------------------------------
// Process list
// ---------------------------------------------------------------------------

function getProcessListViaPowerShell(): Map<number, ProcessInfo> {
    const map = new Map<number, ProcessInfo>();
    const psScript = [
        '$ErrorActionPreference = "SilentlyContinue"',
        'Get-Process | ForEach-Object {',
        '  $p = ""; try { $p = $_.MainModule.FileName } catch {}',
        '  [PSCustomObject]@{ Id = [int]$_.Id; Name = $_.ProcessName; Path = $p; Session = [int]$_.SessionId }',
        '} | ConvertTo-Csv -NoTypeInformation'
    ].join(' ');

    const output = runCommand(
        `powershell -NoProfile -NonInteractive -Command "${psScript}"`
    );

    const lines = output.split(/\r?\n/).map(l => l.trim()).filter(l => l.length > 0);
    if (lines.length < 2) { return map; }

    const header = lines[0].toLowerCase().replace(/"/g, '').split(',');
    const idIdx      = header.indexOf('id');
    const nameIdx    = header.indexOf('name');
    const pathIdx    = header.indexOf('path');
    const sessionIdx = header.indexOf('session');

    if (idIdx < 0 || nameIdx < 0) { return map; }

    for (let i = 1; i < lines.length; i++) {
        const cols = lines[i].split(',').map(c => c.replace(/^"|"$/g, '').trim());
        if (cols.length <= Math.max(idIdx, nameIdx)) { continue; }
        const pid = parseInt(cols[idIdx], 10);
        if (isNaN(pid)) { continue; }
        map.set(pid, {
            name:           cols[nameIdx]  ?? 'Unknown',
            executablePath: pathIdx    >= 0 ? (cols[pathIdx]    ?? '') : '',
            sessionId:      sessionIdx >= 0 ? (parseInt(cols[sessionIdx], 10) || 0) : 0
        });
    }

    return map;
}

function getProcessListViaWmic(): Map<number, ProcessInfo> {
    const map = new Map<number, ProcessInfo>();

    // wmic CSV: first column is Node; remaining columns are alphabetically sorted
    const output = runCommand(
        'wmic process get ExecutablePath,Name,ProcessId,SessionId /format:csv'
    );

    const lines = output.split(/\r?\n/).map(l => l.trim()).filter(l => l.length > 0);
    if (lines.length < 2) { return map; }

    const header = lines[0].toLowerCase().split(',');
    const execIdx    = header.indexOf('executablepath');
    const nameIdx    = header.indexOf('name');
    const pidIdx     = header.indexOf('processid');
    const sessionIdx = header.indexOf('sessionid');

    if (pidIdx < 0 || nameIdx < 0) { return map; }

    for (let i = 1; i < lines.length; i++) {
        const cols = lines[i].split(',');
        if (cols.length <= Math.max(pidIdx, nameIdx)) { continue; }
        const pid = parseInt(cols[pidIdx]?.trim() ?? '', 10);
        if (isNaN(pid)) { continue; }
        map.set(pid, {
            name:           cols[nameIdx]?.trim()    ?? 'Unknown',
            executablePath: execIdx    >= 0 ? (cols[execIdx]?.trim()    ?? '') : '',
            sessionId:      sessionIdx >= 0 ? (parseInt(cols[sessionIdx]?.trim() ?? '', 10) || 0) : 0
        });
    }

    return map;
}

function getProcessList(): Map<number, ProcessInfo> {
    try { return getProcessListViaPowerShell(); } catch { /* fall through */ }
    try { return getProcessListViaWmic();       } catch { /* fall through */ }
    return new Map();
}

// ---------------------------------------------------------------------------
// Netstat parsing
// ---------------------------------------------------------------------------

function parseEndpoint(endpoint: string): { address: string; port: number } | null {
    if (endpoint === '*:*') { return { address: '*', port: 0 }; }

    if (endpoint.startsWith('[')) {
        const close = endpoint.indexOf(']');
        if (close < 0 || close + 2 > endpoint.length) { return null; }
        const port = parseInt(endpoint.substring(close + 2), 10);
        return isNaN(port) ? null : { address: endpoint.substring(1, close), port };
    }

    const lastColon = endpoint.lastIndexOf(':');
    if (lastColon <= 0 || lastColon + 1 >= endpoint.length) { return null; }
    const port = parseInt(endpoint.substring(lastColon + 1), 10);
    return isNaN(port) ? null : { address: endpoint.substring(0, lastColon), port };
}

function parseNetstat(): NetstatRow[] {
    const rows: NetstatRow[] = [];
    try {
        const output = runCommand('netstat -ano');
        for (const line of output.split('\n')) {
            const parts = line.trim().split(/\s+/);
            const proto = parts[0]?.toUpperCase();

            if (proto === 'TCP' && parts.length >= 5) {
                const local  = parseEndpoint(parts[1]);
                const remote = parseEndpoint(parts[2]);
                // Only listening endpoints (remote port == 0 means no active connection)
                if (!local || !remote || remote.port !== 0) { continue; }
                const pid = parseInt(parts[4], 10);
                if (isNaN(pid)) { continue; }
                rows.push({ protocol: 'TCP', localAddress: local.address, port: local.port, state: parts[3], processId: pid });

            } else if (proto === 'UDP' && parts.length >= 4) {
                const local = parseEndpoint(parts[1]);
                if (!local) { continue; }
                const pid = parseInt(parts[3], 10);
                if (isNaN(pid)) { continue; }
                rows.push({ protocol: 'UDP', localAddress: local.address, port: local.port, state: 'Listening', processId: pid });
            }
        }
    } catch { /* return whatever was collected */ }
    return rows;
}

// ---------------------------------------------------------------------------
// Protection evaluation  (mirrors ProcessProtectionService.cs)
// ---------------------------------------------------------------------------

function evaluateProtection(
    pid: number,
    name: string,
    executablePath: string,
    sessionId: number,
    currentPid: number
): { isProtected: boolean; reason: string } {
    if (pid <= 4) {
        return { isProtected: true, reason: 'Core Windows process. Stop is blocked.' };
    }
    if (pid === currentPid) {
        return { isProtected: true, reason: 'This extension cannot stop itself.' };
    }
    const baseName = name.replace(/\.exe$/i, '').toLowerCase();
    if (CRITICAL_PROCESS_NAMES.has(baseName)) {
        return { isProtected: true, reason: 'Known Windows critical process. Stop is blocked.' };
    }
    if (sessionId === 0) {
        return { isProtected: true, reason: 'Windows service session detected. Stop is blocked.' };
    }
    if (executablePath && executablePath.toLowerCase().startsWith(WINDOWS_DIR)) {
        return { isProtected: true, reason: 'Process runs from the Windows folder. Stop is blocked.' };
    }
    return { isProtected: false, reason: 'User process. Stop is allowed.' };
}

// ---------------------------------------------------------------------------
// Public API
// ---------------------------------------------------------------------------

export function getEntries(catalog: DevPortCatalog): PortProcessEntry[] {
    const netstatRows = parseNetstat();
    const processList = getProcessList();
    const currentPid  = process.pid;

    const entries: PortProcessEntry[] = netstatRows.map(row => {
        const info           = processList.get(row.processId);
        const name           = info?.name ?? 'Unknown';
        const executablePath = info?.executablePath ?? '';
        const sessionId      = info?.sessionId ?? -1;

        const protection = evaluateProtection(row.processId, name, executablePath, sessionId, currentPid);
        const profile    = catalog.match(row.port, name, executablePath);
        const isDev      = profile.key !== '';

        return {
            protocol:        row.protocol,
            localAddress:    row.localAddress,
            port:            row.port,
            state:           row.state,
            processId:       row.processId,
            processName:     name,
            executablePath:  executablePath || 'Unavailable',
            isProtected:     protection.isProtected,
            protectionReason: protection.reason,
            devProfileKey:   profile.key,
            devProfileLabel: profile.label,
            safetyLabel:     protection.isProtected ? 'Protected' : 'Safe to stop',
            isDevelopmentPort: isDev
        };
    });

    return entries.sort((a, b) => {
        if (a.isDevelopmentPort !== b.isDevelopmentPort) {
            return a.isDevelopmentPort ? -1 : 1;
        }
        return a.port - b.port || a.processName.localeCompare(b.processName);
    });
}

export function killProcess(pid: number): void {
    if (!Number.isInteger(pid) || pid <= 0) {
        throw new Error(`Invalid PID: ${pid}`);
    }
    cp.execSync(`taskkill /PID ${pid} /F`, { windowsHide: true, timeout: 6000 });
}
