"use strict";
var __createBinding = (this && this.__createBinding) || (Object.create ? (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    var desc = Object.getOwnPropertyDescriptor(m, k);
    if (!desc || ("get" in desc ? !m.__esModule : desc.writable || desc.configurable)) {
      desc = { enumerable: true, get: function() { return m[k]; } };
    }
    Object.defineProperty(o, k2, desc);
}) : (function(o, m, k, k2) {
    if (k2 === undefined) k2 = k;
    o[k2] = m[k];
}));
var __setModuleDefault = (this && this.__setModuleDefault) || (Object.create ? (function(o, v) {
    Object.defineProperty(o, "default", { enumerable: true, value: v });
}) : function(o, v) {
    o["default"] = v;
});
var __importStar = (this && this.__importStar) || (function () {
    var ownKeys = function(o) {
        ownKeys = Object.getOwnPropertyNames || function (o) {
            var ar = [];
            for (var k in o) if (Object.prototype.hasOwnProperty.call(o, k)) ar[ar.length] = k;
            return ar;
        };
        return ownKeys(o);
    };
    return function (mod) {
        if (mod && mod.__esModule) return mod;
        var result = {};
        if (mod != null) for (var k = ownKeys(mod), i = 0; i < k.length; i++) if (k[i] !== "default") __createBinding(result, mod, k[i]);
        __setModuleDefault(result, mod);
        return result;
    };
})();
Object.defineProperty(exports, "__esModule", { value: true });
exports.AnclaSidebarProvider = exports.AnclaPanel = void 0;
const vscode = __importStar(require("vscode"));
const portService_1 = require("./portService");
const devPortCatalog_1 = require("./devPortCatalog");
// ---------------------------------------------------------------------------
// Panel provider
// ---------------------------------------------------------------------------
class AnclaPanel {
    static currentPanel;
    static VIEW_TYPE = 'ancla.portManager';
    _panel;
    _extensionUri;
    _catalog = new devPortCatalog_1.DevPortCatalog();
    _disposables = [];
    _isRefreshing = false;
    // -------------------------------------------------------------------------
    constructor(panel, extensionUri) {
        this._panel = panel;
        this._extensionUri = extensionUri;
        this._panel.webview.html = this._buildHtml(this._panel.webview);
        this._panel.webview.onDidReceiveMessage((msg) => this._handleMessage(msg), null, this._disposables);
        this._panel.onDidDispose(() => this.dispose(), null, this._disposables);
    }
    // -------------------------------------------------------------------------
    static createOrShow(context) {
        const column = vscode.window.activeTextEditor
            ? vscode.ViewColumn.Beside
            : vscode.ViewColumn.One;
        if (AnclaPanel.currentPanel) {
            AnclaPanel.currentPanel._panel.reveal(column);
            return;
        }
        const panel = vscode.window.createWebviewPanel(AnclaPanel.VIEW_TYPE, 'Ancla – Port Manager', column, {
            enableScripts: true,
            retainContextWhenHidden: true,
            localResourceRoots: [vscode.Uri.joinPath(context.extensionUri, 'media')]
        });
        AnclaPanel.currentPanel = new AnclaPanel(panel, context.extensionUri);
    }
    // -------------------------------------------------------------------------
    async _handleMessage(msg) {
        switch (msg.type) {
            case 'ready':
            case 'refresh':
                await this._refresh();
                break;
            case 'killRequest':
                await this._handleKillRequest(msg.pid, msg.confirmMsg);
                break;
        }
    }
    async _refresh() {
        if (this._isRefreshing) {
            return;
        }
        this._isRefreshing = true;
        await this._post({ type: 'status', message: 'Refreshing ports…' });
        try {
            const entries = await new Promise((resolve, reject) => {
                setImmediate(() => {
                    try {
                        resolve((0, portService_1.getEntries)(this._catalog));
                    }
                    catch (e) {
                        reject(e);
                    }
                });
            });
            await this._post({
                type: 'update',
                entries,
                filters: this._catalog.getFilters()
            });
        }
        catch (e) {
            const msg = e instanceof Error ? e.message : String(e);
            await this._post({ type: 'error', message: msg });
        }
        finally {
            this._isRefreshing = false;
        }
    }
    async _handleKillRequest(pid, confirmMsg) {
        const answer = await vscode.window.showWarningMessage(confirmMsg, { modal: true }, 'Yes');
        if (answer !== 'Yes') {
            return;
        }
        try {
            (0, portService_1.killProcess)(pid);
            await this._post({ type: 'status', message: `Stopped PID ${pid}` });
            await this._refresh();
        }
        catch (e) {
            const msg = e instanceof Error ? e.message : String(e);
            vscode.window.showErrorMessage(`Ancla: could not stop PID ${pid} — ${msg}`);
            await this._post({ type: 'error', message: msg });
        }
    }
    _post(message) {
        return this._panel.webview.postMessage(message);
    }
    // -------------------------------------------------------------------------
    dispose() {
        AnclaPanel.currentPanel = undefined;
        this._panel.dispose();
        for (const d of this._disposables) {
            d.dispose();
        }
        this._disposables.length = 0;
    }
    // -------------------------------------------------------------------------
    _buildHtml(webview) {
        const scriptUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'main.js'));
        const styleUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'styles.css'));
        const nonce = getNonce();
        return /* html */ `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta http-equiv="Content-Security-Policy"
        content="default-src 'none';
                 style-src ${webview.cspSource} 'unsafe-inline';
                 script-src 'nonce-${nonce}';">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <link href="${styleUri}" rel="stylesheet">
  <title>Ancla</title>
</head>
<body>
  <div id="app">

    <!-- ── Header ── -->
    <div id="header">
      <div id="header-left">
        <h1 id="title">Ancla</h1>
        <span id="subtitle"></span>
      </div>
      <div id="header-right">
        <select id="language-picker" title="Language / Idioma">
          <option value="en">English</option>
          <option value="es">Español</option>
        </select>
        <button id="refresh-btn" class="btn btn-primary"></button>
        <label class="toggle-label">
          <input type="checkbox" id="auto-refresh">
          <span id="auto-refresh-label"></span>
        </label>
      </div>
    </div>

    <!-- ── Toolbar ── -->
    <div id="toolbar">
      <div id="search-bar">
        <span id="search-label"></span>
        <input type="text" id="search-input">
      </div>
      <label class="toggle-label">
        <input type="checkbox" id="show-protected">
        <span id="show-protected-label"></span>
      </label>
    </div>

    <!-- ── Quick filters ── -->
    <div id="filters-bar">
      <span id="filters-label"></span>
      <div id="filter-buttons"></div>
    </div>

    <!-- ── Table ── -->
    <div id="table-wrap">
      <table id="ports-table">
        <thead>
          <tr>
            <th class="col-icon"></th>
            <th class="sortable" data-col="devProfileLabel" id="th-dev"></th>
            <th class="sortable" data-col="port"           id="th-port"></th>
            <th class="sortable" data-col="protocol"       id="th-protocol"></th>
            <th class="sortable" data-col="localAddress"   id="th-address"></th>
            <th class="sortable" data-col="processName"    id="th-process"></th>
            <th class="sortable" data-col="processId"      id="th-pid"></th>
            <th class="sortable" data-col="state"          id="th-state"></th>
            <th class="sortable" data-col="safetyLabel"    id="th-safety"></th>
            <th class="sortable" data-col="executablePath" id="th-path"></th>
          </tr>
        </thead>
        <tbody id="ports-body"></tbody>
      </table>
    </div>

    <!-- ── Detail panel ── -->
    <div id="detail-panel">
      <div id="detail-info">
        <span id="detail-port"></span>
        <span id="detail-process"></span>
        <span id="detail-path"></span>
        <span id="detail-protection"></span>
      </div>
      <button id="stop-btn" class="btn btn-danger" disabled></button>
    </div>

    <!-- ── Status bar ── -->
    <div id="status-bar">
      <span id="status-message"></span>
      <span id="result-count"></span>
    </div>

  </div>
  <script nonce="${nonce}" src="${scriptUri}"></script>
</body>
</html>`;
    }
}
exports.AnclaPanel = AnclaPanel;
// ---------------------------------------------------------------------------
// Sidebar (WebviewView) provider
// ---------------------------------------------------------------------------
class AnclaSidebarProvider {
    _extensionUri;
    static VIEW_ID = 'ancla.sidebarView';
    _view;
    _catalog = new devPortCatalog_1.DevPortCatalog();
    _isRefreshing = false;
    constructor(_extensionUri) {
        this._extensionUri = _extensionUri;
    }
    resolveWebviewView(view) {
        this._view = view;
        view.webview.options = {
            enableScripts: true,
            localResourceRoots: [vscode.Uri.joinPath(this._extensionUri, 'media')]
        };
        view.webview.html = this._buildHtml(view.webview);
        view.webview.onDidReceiveMessage((msg) => this._handleMessage(msg));
    }
    async _handleMessage(msg) {
        switch (msg.type) {
            case 'ready':
            case 'refresh':
                await this._refresh();
                break;
            case 'killRequest':
                await this._handleKillRequest(msg.pid, msg.confirmMsg);
                break;
        }
    }
    async _refresh() {
        if (this._isRefreshing || !this._view) {
            return;
        }
        this._isRefreshing = true;
        await this._post({ type: 'status', message: 'Refreshing ports…' });
        try {
            const entries = await new Promise((resolve, reject) => {
                setImmediate(() => {
                    try {
                        resolve((0, portService_1.getEntries)(this._catalog));
                    }
                    catch (e) {
                        reject(e);
                    }
                });
            });
            await this._post({ type: 'update', entries, filters: this._catalog.getFilters() });
        }
        catch (e) {
            const msg = e instanceof Error ? e.message : String(e);
            await this._post({ type: 'error', message: msg });
        }
        finally {
            this._isRefreshing = false;
        }
    }
    async _handleKillRequest(pid, confirmMsg) {
        const answer = await vscode.window.showWarningMessage(confirmMsg, { modal: true }, 'Yes');
        if (answer !== 'Yes') {
            return;
        }
        try {
            (0, portService_1.killProcess)(pid);
            await this._post({ type: 'status', message: `Stopped PID ${pid}` });
            await this._refresh();
        }
        catch (e) {
            const msg = e instanceof Error ? e.message : String(e);
            vscode.window.showErrorMessage(`Ancla: could not stop PID ${pid} — ${msg}`);
            await this._post({ type: 'error', message: msg });
        }
    }
    _post(message) {
        return this._view.webview.postMessage(message);
    }
    _buildHtml(webview) {
        const scriptUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'main.js'));
        const styleUri = webview.asWebviewUri(vscode.Uri.joinPath(this._extensionUri, 'media', 'styles.css'));
        const nonce = getNonce();
        return /* html */ `<!DOCTYPE html>
<html lang="en">
<head>
  <meta charset="UTF-8">
  <meta http-equiv="Content-Security-Policy"
        content="default-src 'none';
                 style-src ${webview.cspSource} 'unsafe-inline';
                 script-src 'nonce-${nonce}';">
  <meta name="viewport" content="width=device-width, initial-scale=1.0">
  <link href="${styleUri}" rel="stylesheet">
  <title>Ancla</title>
</head>
<body>
  <div id="app">
    <div id="header">
      <div id="header-left">
        <h1 id="title">Ancla</h1>
        <span id="subtitle"></span>
      </div>
      <div id="header-right">
        <select id="language-picker" title="Language / Idioma">
          <option value="en">English</option>
          <option value="es">Español</option>
        </select>
        <button id="refresh-btn" class="btn btn-primary"></button>
        <label class="toggle-label">
          <input type="checkbox" id="auto-refresh">
          <span id="auto-refresh-label"></span>
        </label>
      </div>
    </div>
    <div id="toolbar">
      <div id="search-bar">
        <span id="search-label"></span>
        <input type="text" id="search-input">
      </div>
      <label class="toggle-label">
        <input type="checkbox" id="show-protected">
        <span id="show-protected-label"></span>
      </label>
    </div>
    <div id="filters-bar">
      <span id="filters-label"></span>
      <div id="filter-buttons"></div>
    </div>
    <div id="table-wrap">
      <table id="ports-table">
        <thead>
          <tr>
            <th class="col-icon"></th>
            <th class="sortable" data-col="devProfileLabel" id="th-dev"></th>
            <th class="sortable" data-col="port"           id="th-port"></th>
            <th class="sortable" data-col="protocol"       id="th-protocol"></th>
            <th class="sortable" data-col="localAddress"   id="th-address"></th>
            <th class="sortable" data-col="processName"    id="th-process"></th>
            <th class="sortable" data-col="processId"      id="th-pid"></th>
            <th class="sortable" data-col="state"          id="th-state"></th>
            <th class="sortable" data-col="safetyLabel"    id="th-safety"></th>
            <th class="sortable" data-col="executablePath" id="th-path"></th>
          </tr>
        </thead>
        <tbody id="ports-body"></tbody>
      </table>
    </div>
    <div id="detail-panel">
      <div id="detail-info">
        <span id="detail-port"></span>
        <span id="detail-process"></span>
        <span id="detail-path"></span>
        <span id="detail-protection"></span>
      </div>
      <button id="stop-btn" class="btn btn-danger" disabled></button>
    </div>
    <div id="status-bar">
      <span id="status-message"></span>
      <span id="result-count"></span>
    </div>
  </div>
  <script nonce="${nonce}" src="${scriptUri}"></script>
</body>
</html>`;
    }
}
exports.AnclaSidebarProvider = AnclaSidebarProvider;
// ---------------------------------------------------------------------------
function getNonce() {
    const chars = 'ABCDEFGHIJKLMNOPQRSTUVWXYZabcdefghijklmnopqrstuvwxyz0123456789';
    return Array.from({ length: 32 }, () => chars[Math.floor(Math.random() * chars.length)]).join('');
}
//# sourceMappingURL=panelProvider.js.map