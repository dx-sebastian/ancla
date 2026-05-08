using System.Diagnostics;

namespace Ancla.Services;

public sealed class ProcessProtectionService
{
    private static readonly HashSet<string> CriticalNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "csrss",
        "dwm",
        "fontdrvhost",
        "idle",
        "lsass",
        "services",
        "smss",
        "spoolsv",
        "svchost",
        "system",
        "wininit",
        "winlogon"
    };

    private readonly int _currentProcessId = Environment.ProcessId;
    private readonly string _windowsDirectory = Path.GetFullPath(Environment.GetFolderPath(Environment.SpecialFolder.Windows));

    public ProcessDetails GetProcessDetails(int processId)
    {
        try
        {
            using var process = Process.GetProcessById(processId);
            var processName = SafeRead(() => process.ProcessName, "Unknown process");
            var executablePath = SafeRead(() => process.MainModule?.FileName ?? string.Empty, string.Empty);
            var sessionId = SafeRead(() => process.SessionId, -1);

            var protection = Evaluate(processId, processName, executablePath, sessionId);
            return new ProcessDetails(
                processId,
                processName,
                string.IsNullOrWhiteSpace(executablePath) ? "Unavailable" : executablePath,
                protection.IsProtected,
                protection.Reason);
        }
        catch
        {
            return new ProcessDetails(
                processId,
                "Exited or unavailable",
                "Unavailable",
                true,
                "The process is unavailable, so stop is blocked.");
        }
    }

    private ProtectionResult Evaluate(int processId, string processName, string executablePath, int sessionId)
    {
        if (processId <= 4)
        {
            return new ProtectionResult(true, "Core Windows process. Stop is blocked.");
        }

        if (processId == _currentProcessId)
        {
            return new ProtectionResult(true, "This app cannot stop itself.");
        }

        if (CriticalNames.Contains(processName))
        {
            return new ProtectionResult(true, "Known Windows critical process. Stop is blocked.");
        }

        if (sessionId == 0)
        {
            return new ProtectionResult(true, "Windows service session detected. Stop is blocked.");
        }

        if (!string.IsNullOrWhiteSpace(executablePath))
        {
            var fullPath = Path.GetFullPath(executablePath);
            if (fullPath.StartsWith(_windowsDirectory, StringComparison.OrdinalIgnoreCase))
            {
                return new ProtectionResult(true, "Process runs from the Windows folder. Stop is blocked.");
            }
        }

        return new ProtectionResult(false, "User process. Stop is allowed.");
    }

    private static T SafeRead<T>(Func<T> readValue, T fallback)
    {
        try
        {
            return readValue();
        }
        catch
        {
            return fallback;
        }
    }

    private sealed record ProtectionResult(bool IsProtected, string Reason);
}

public sealed record ProcessDetails(
    int ProcessId,
    string ProcessName,
    string ExecutablePath,
    bool IsProtected,
    string ProtectionReason);
