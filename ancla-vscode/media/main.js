// @ts-check
// Ancla – Port Manager  |  WebView frontend
(function () {
    'use strict';

    // -------------------------------------------------------------------------
    // VS Code API (must be called exactly once)
    // -------------------------------------------------------------------------
    // @ts-ignore
    const vscode = acquireVsCodeApi();

    // -------------------------------------------------------------------------
    // State
    // -------------------------------------------------------------------------
    /** @type {any[]} */
    let allEntries = [];
    /** @type {any[]} */
    let allFilters = [];
    let selectedFilter = 'all';
    /** @type {any|null} */
    let selectedEntry = null;
    let language = 'en';
    let sortCol = 'port';
    let sortAsc = true;
    /** @type {ReturnType<typeof setInterval>|null} */
    let autoRefreshTimer = null;

    // -------------------------------------------------------------------------
    // Translations  (mirrors UiTextCatalog.cs)
    // -------------------------------------------------------------------------
    const TEXTS = {
        en: {
            subtitle:         'See your ports',
            refreshNow:       'Refresh now',
            autoRefreshOn:    'Auto refresh every 5 seconds',
            autoRefreshOff:   'Auto refresh is off',
            search:           'Search',
            searchPlaceholder:'Port, process, PID, framework or path',
            quickFilters:     'Quick filters',
            showProtected:    'Show protected system ports',
            stopProcess:      'Stop selected process',
            readyToScan:      'Ready to scan',
            noPortSelected:   'No port selected',
            chooseRowHint:    'Choose a row to inspect a process',
            pathLabel:        'Path',
            protectionLabel:  'Protection',
            shownCount:       (/** @type {number} */ n) => `${n} shown`,
            portsLoaded:      (/** @type {number} */ n) => `${n} ports loaded`,
            confirmStop:      (/** @type {string} */ name, /** @type {number} */ pid, /** @type {number} */ port) =>
                                `Stop ${name} (PID ${pid}) using port ${port}?`,
            devCol:      'Dev',
            portCol:     'Port',
            protocolCol: 'Protocol',
            addressCol:  'Address',
            processCol:  'Process',
            pidCol:      'PID',
            stateCol:    'State',
            safetyCol:   'Safety',
            pathCol:     'Path',
        },
        es: {
            subtitle:         'Mira tus puertos',
            refreshNow:       'Actualizar',
            autoRefreshOn:    'Auto actualizar cada 5 segundos',
            autoRefreshOff:   'Auto actualizar apagado',
            search:           'Buscar',
            searchPlaceholder:'Puerto, proceso, PID, framework o ruta',
            quickFilters:     'Filtros rápidos',
            showProtected:    'Mostrar puertos protegidos',
            stopProcess:      'Detener proceso',
            readyToScan:      'Listo para escanear',
            noPortSelected:   'Ningún puerto seleccionado',
            chooseRowHint:    'Elige una fila para inspeccionar un proceso',
            pathLabel:        'Ruta',
            protectionLabel:  'Protección',
            shownCount:       (/** @type {number} */ n) => `${n} visibles`,
            portsLoaded:      (/** @type {number} */ n) => `${n} puertos cargados`,
            confirmStop:      (/** @type {string} */ name, /** @type {number} */ pid, /** @type {number} */ port) =>
                                `¿Detener ${name} (PID ${pid}) usando el puerto ${port}?`,
            devCol:      'Dev',
            portCol:     'Puerto',
            protocolCol: 'Protocolo',
            addressCol:  'Dirección',
            processCol:  'Proceso',
            pidCol:      'PID',
            stateCol:    'Estado',
            safetyCol:   'Seguridad',
            pathCol:     'Ruta',
        }
    };

    /**
     * @param {string} key
     * @param {any[]} args
     */
    function t(key, ...args) {
        const dict = TEXTS[language] || TEXTS.en;
        const val = dict[key];
        if (typeof val === 'function') { return val(...args); }
        return val ?? key;
    }

    // -------------------------------------------------------------------------
    // Dev profile badge colours
    // -------------------------------------------------------------------------
    const BADGE_COLORS = {
        react:        '#61dafb',
        vue:          '#42b883',
        angular:      '#dd0031',
        docker:       '#2496ed',
        sails:        '#14acc2',
        'ant-design': '#1890ff',
        storybook:    '#ff4785',
        aspnet:       '#512bd4',
    };

    const BADGE_LABELS = {
        react:        'Re',
        vue:          'Vu',
        angular:      'Ng',
        docker:       'Dk',
        sails:        'Sl',
        'ant-design': 'AD',
        storybook:    'SB',
        aspnet:       '.N',
    };

    /** @param {string} key */
    function devBadgeHtml(key) {
        const color = BADGE_COLORS[key] || '#6c757d';
        const label = BADGE_LABELS[key] || 'D';
        const textColor = ['react', 'docker'].includes(key) ? '#1a1a2e' : '#fff';
        return `<span class="dev-badge" style="background:${color};color:${textColor}" title="${escHtml(key)}">${label}</span>`;
    }

    // -------------------------------------------------------------------------
    // DOM refs
    // -------------------------------------------------------------------------
    const $ = (/** @type {string} */ id) => document.getElementById(id);

    const subtitleEl       = $('subtitle');
    const refreshBtn       = $('refresh-btn');
    const autoRefreshCb    = /** @type {HTMLInputElement} */ ($('auto-refresh'));
    const autoRefreshLabel = $('auto-refresh-label');
    const langPicker       = /** @type {HTMLSelectElement} */ ($('language-picker'));
    const searchLabel      = $('search-label');
    const searchInput      = /** @type {HTMLInputElement} */ ($('search-input'));
    const filtersLabel     = $('filters-label');
    const filterButtons    = $('filter-buttons');
    const showProtectedCb  = /** @type {HTMLInputElement} */ ($('show-protected'));
    const showProtectedLbl = $('show-protected-label');
    const portsBody        = $('ports-body');
    const detailPort       = $('detail-port');
    const detailProcess    = $('detail-process');
    const detailPath       = $('detail-path');
    const detailProtection = $('detail-protection');
    const stopBtn          = $('stop-btn');
    const statusMsg        = $('status-message');
    const resultCount      = $('result-count');
    const theadRow         = document.querySelector('#ports-table thead tr');

    // -------------------------------------------------------------------------
    // Language / UI text
    // -------------------------------------------------------------------------
    function applyLanguage() {
        subtitleEl.textContent         = t('subtitle');
        refreshBtn.textContent         = t('refreshNow');
        autoRefreshLabel.textContent   = autoRefreshCb.checked ? t('autoRefreshOn') : t('autoRefreshOff');
        searchLabel.textContent        = t('search');
        searchInput.placeholder        = t('searchPlaceholder');
        filtersLabel.textContent       = t('quickFilters');
        showProtectedLbl.textContent   = t('showProtected');
        stopBtn.textContent            = t('stopProcess');

        // Column headers
        $('th-dev').textContent      = t('devCol');
        $('th-port').textContent     = t('portCol');
        $('th-protocol').textContent = t('protocolCol');
        $('th-address').textContent  = t('addressCol');
        $('th-process').textContent  = t('processCol');
        $('th-pid').textContent      = t('pidCol');
        $('th-state').textContent    = t('stateCol');
        $('th-safety').textContent   = t('safetyCol');
        $('th-path').textContent     = t('pathCol');

        // Filter button labels
        allFilters.forEach(f => {
            const btn = filterButtons.querySelector(`[data-key="${f.key}"]`);
            if (btn) { btn.textContent = language === 'es' ? f.labelEs : f.labelEn; }
        });

        updateDetails();
    }

    // -------------------------------------------------------------------------
    // Filter buttons
    // -------------------------------------------------------------------------
    function buildFilterButtons() {
        filterButtons.innerHTML = '';
        allFilters.forEach(f => {
            const btn = document.createElement('button');
            btn.className = 'filter-btn';
            btn.dataset.key = f.key;
            btn.textContent = language === 'es' ? f.labelEs : f.labelEn;
            if (f.key === selectedFilter) { btn.classList.add('active'); }
            btn.addEventListener('click', () => {
                selectedFilter = f.key;
                filterButtons.querySelectorAll('.filter-btn').forEach(b => b.classList.remove('active'));
                btn.classList.add('active');
                renderTable();
            });
            filterButtons.appendChild(btn);
        });
    }

    // -------------------------------------------------------------------------
    // Table rendering
    // -------------------------------------------------------------------------
    /** @param {string} s */
    function escHtml(s) {
        return String(s ?? '')
            .replace(/&/g, '&amp;')
            .replace(/</g, '&lt;')
            .replace(/>/g, '&gt;')
            .replace(/"/g, '&quot;');
    }

    function renderTable() {
        const query        = searchInput.value.trim().toLowerCase();
        const showProtected = showProtectedCb.checked;

        let filtered = allEntries.filter(e => {
            if (!showProtected && e.isProtected) { return false; }
            if (selectedFilter === 'dev-only' && !e.isDevelopmentPort) { return false; }
            if (selectedFilter !== 'all' && selectedFilter !== 'dev-only') {
                if ((e.devProfileKey || '').toLowerCase() !== selectedFilter.toLowerCase()) { return false; }
            }
            if (query) {
                const fields = [
                    String(e.port), String(e.processId),
                    e.processName.toLowerCase(), e.localAddress.toLowerCase(),
                    (e.executablePath || '').toLowerCase(),
                    (e.devProfileLabel || '').toLowerCase()
                ];
                if (!fields.some(f => f.includes(query))) { return false; }
            }
            return true;
        });

        // Sort
        filtered.sort((a, b) => {
            const av = a[sortCol];
            const bv = b[sortCol];
            if (typeof av === 'number' && typeof bv === 'number') {
                return sortAsc ? av - bv : bv - av;
            }
            const as = String(av ?? '').toLowerCase();
            const bs = String(bv ?? '').toLowerCase();
            return sortAsc ? as.localeCompare(bs) : bs.localeCompare(as);
        });

        // Build HTML in one pass for performance
        const rows = filtered.map(entry => {
            const isSelected = selectedEntry
                && selectedEntry.processId === entry.processId
                && selectedEntry.port      === entry.port;

            let rowClass = '';
            if (isSelected)         { rowClass = 'selected'; }
            else if (entry.isProtected)    { rowClass = 'row-protected'; }
            else if (entry.isDevelopmentPort) { rowClass = 'row-dev'; }

            const safetyClass = entry.isProtected ? 'safety-protected' : 'safety-safe';
            const badge = entry.isDevelopmentPort ? devBadgeHtml(entry.devProfileKey) : '';

            return `<tr class="${rowClass}"
                        data-pid="${entry.processId}"
                        data-port="${entry.port}"
                        data-idx="${allEntries.indexOf(entry)}">
                <td class="col-icon">${badge}</td>
                <td class="col-dev"      data-label="${escHtml(t('devCol'))}">${escHtml(entry.devProfileLabel)}</td>
                <td class="col-port"     data-label="${escHtml(t('portCol'))}">${entry.port}</td>
                <td class="col-protocol" data-label="${escHtml(t('protocolCol'))}">${escHtml(entry.protocol)}</td>
                <td class="col-address"  data-label="${escHtml(t('addressCol'))}">${escHtml(entry.localAddress)}</td>
                <td class="col-process"  data-label="${escHtml(t('processCol'))}">${escHtml(entry.processName)}</td>
                <td class="col-pid"      data-label="${escHtml(t('pidCol'))}">${entry.processId}</td>
                <td class="col-state"    data-label="${escHtml(t('stateCol'))}">${escHtml(entry.state)}</td>
                <td class="col-safety ${safetyClass}" data-label="${escHtml(t('safetyCol'))}">${escHtml(entry.safetyLabel)}</td>
                <td class="col-path"     data-label="${escHtml(t('pathCol'))}" title="${escHtml(entry.executablePath)}">${escHtml(entry.executablePath)}</td>
            </tr>`;
        }).join('');

        portsBody.innerHTML = rows;

        // Attach click handlers after innerHTML (event delegation)
        portsBody.querySelectorAll('tr').forEach((tr, i) => {
            tr.addEventListener('click', () => {
                selectedEntry = filtered[i];
                portsBody.querySelectorAll('tr').forEach(r => r.classList.remove('selected'));
                tr.classList.add('selected');
                updateDetails();
            });
        });

        resultCount.textContent = t('shownCount', filtered.length);
        updateDetails();
    }

    // -------------------------------------------------------------------------
    // Detail panel
    // -------------------------------------------------------------------------
    function updateDetails() {
        if (!selectedEntry) {
            detailPort.textContent       = t('noPortSelected');
            detailProcess.textContent    = t('chooseRowHint');
            detailPath.textContent       = `${t('pathLabel')}: -`;
            detailProtection.textContent = `${t('protectionLabel')}: -`;
            stopBtn.disabled             = true;
            return;
        }
        const e = selectedEntry;
        detailPort.textContent       = `${e.protocol} ${e.localAddress}:${e.port}`;
        detailProcess.textContent    = `${e.processName} (PID ${e.processId})`;
        detailPath.textContent       = `${t('pathLabel')}: ${e.executablePath}`;
        detailProtection.textContent = `${t('protectionLabel')}: ${e.protectionReason}`;
        stopBtn.disabled             = e.isProtected;
    }

    // -------------------------------------------------------------------------
    // Event listeners
    // -------------------------------------------------------------------------
    refreshBtn.addEventListener('click', () => {
        vscode.postMessage({ type: 'refresh' });
    });

    searchInput.addEventListener('input', renderTable);

    showProtectedCb.addEventListener('change', renderTable);

    autoRefreshCb.addEventListener('change', () => {
        autoRefreshLabel.textContent = autoRefreshCb.checked ? t('autoRefreshOn') : t('autoRefreshOff');
        if (autoRefreshCb.checked) {
            autoRefreshTimer = setInterval(() => {
                vscode.postMessage({ type: 'refresh' });
            }, 5000);
        } else {
            if (autoRefreshTimer !== null) {
                clearInterval(autoRefreshTimer);
                autoRefreshTimer = null;
            }
        }
    });

    langPicker.addEventListener('change', () => {
        language = langPicker.value;
        applyLanguage();
        renderTable();
    });

    stopBtn.addEventListener('click', () => {
        if (!selectedEntry || selectedEntry.isProtected) { return; }
        const confirmMsg = t('confirmStop', selectedEntry.processName, selectedEntry.processId, selectedEntry.port);
        vscode.postMessage({
            type:       'killRequest',
            pid:        selectedEntry.processId,
            confirmMsg
        });
    });

    // Column sorting
    theadRow.querySelectorAll('th.sortable').forEach(th => {
        th.addEventListener('click', () => {
            const col = /** @type {HTMLElement} */ (th).dataset.col;
            if (!col) { return; }
            if (sortCol === col) {
                sortAsc = !sortAsc;
            } else {
                sortCol = col;
                sortAsc = true;
            }
            theadRow.querySelectorAll('th.sortable').forEach(h => {
                h.classList.remove('sort-asc', 'sort-desc');
            });
            th.classList.add(sortAsc ? 'sort-asc' : 'sort-desc');
            renderTable();
        });
    });

    // -------------------------------------------------------------------------
    // Message handler (extension → webview)
    // -------------------------------------------------------------------------
    window.addEventListener('message', event => {
        const msg = event.data;
        switch (msg.type) {
            case 'update':
                allEntries  = msg.entries  ?? [];
                allFilters  = msg.filters  ?? [];
                // Preserve selection if the entry still exists
                if (selectedEntry) {
                    selectedEntry = allEntries.find(
                        e => e.processId === selectedEntry.processId && e.port === selectedEntry.port
                    ) ?? null;
                }
                buildFilterButtons();
                statusMsg.textContent = t('portsLoaded', allEntries.length);
                renderTable();
                break;

            case 'status':
                statusMsg.textContent = msg.message;
                break;

            case 'error':
                statusMsg.textContent = `⚠ ${msg.message}`;
                break;
        }
    });

    // -------------------------------------------------------------------------
    // Boot
    // -------------------------------------------------------------------------
    applyLanguage();
    statusMsg.textContent = t('readyToScan');
    vscode.postMessage({ type: 'ready' });
}());
