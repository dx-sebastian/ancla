using System.ComponentModel;
using System.Diagnostics;
using Ancla.Models;
using Ancla.Services;

namespace Ancla;

public partial class Form1 : Form
{
    private readonly BindingSource _rowsSource = new();
    private readonly NetstatPortService _portService = new();
    private readonly DevelopmentPortCatalog _devPortCatalog = new();
    private readonly System.Windows.Forms.Timer _refreshTimer = new();
    private readonly Dictionary<string, Button> _filterButtons = new(StringComparer.OrdinalIgnoreCase);
    private List<PortProcessEntry> _allEntries = [];
    private UiLanguage _language = UiLanguage.Spanish;
    private string _selectedFilter = DevelopmentPortCatalog.AllFilterKey;
    private bool _isRefreshing;

    public Form1()
    {
        InitializeComponent();
        ConfigureGrid();
        ConfigureLanguagePicker();
        BuildFilterButtons();

        _refreshTimer.Interval = 5000;
        _refreshTimer.Tick += async (_, _) => await RefreshEntriesAsync();

        portsGrid.DataSource = _rowsSource;
        searchTextBox.TextChanged += (_, _) => ApplyFilters();
        autoRefreshCheckBox.CheckedChanged += AutoRefreshCheckBox_CheckedChanged;
        showProtectedCheckBox.CheckedChanged += (_, _) => ApplyFilters();
        languageComboBox.SelectedIndexChanged += LanguageComboBox_SelectedIndexChanged;
        portsGrid.SelectionChanged += (_, _) => UpdateSelectionDetails();
        refreshButton.Click += async (_, _) => await RefreshEntriesAsync();
        stopSelectedButton.Click += async (_, _) => await StopSelectedProcessAsync();

        showProtectedCheckBox.Checked = false;
        ApplyLanguage();
        Load += async (_, _) => await RefreshEntriesAsync();
    }

    private void ConfigureGrid()
    {
        portsGrid.AutoGenerateColumns = false;
        portsGrid.Columns.Clear();
        portsGrid.Columns.Add(new DataGridViewImageColumn
        {
            DataPropertyName = nameof(PortProcessEntry.DevProfileIcon),
            HeaderText = string.Empty,
            Name = "DevIcon",
            Width = 42,
            MinimumWidth = 42,
            ImageLayout = DataGridViewImageCellLayout.Zoom
        });
        portsGrid.Columns.Add(CreateTextColumn(nameof(PortProcessEntry.DevProfileLabel), "Dev", 130));
        portsGrid.Columns.Add(CreateTextColumn(nameof(PortProcessEntry.Port), "Port", 70));
        portsGrid.Columns.Add(CreateTextColumn(nameof(PortProcessEntry.Protocol), "Protocol", 80));
        portsGrid.Columns.Add(CreateTextColumn(nameof(PortProcessEntry.LocalAddress), "Address", 130));
        portsGrid.Columns.Add(CreateTextColumn(nameof(PortProcessEntry.ProcessName), "Process", 180));
        portsGrid.Columns.Add(CreateTextColumn(nameof(PortProcessEntry.ProcessId), "PID", 70));
        portsGrid.Columns.Add(CreateTextColumn(nameof(PortProcessEntry.State), "State", 90));
        portsGrid.Columns.Add(CreateTextColumn(nameof(PortProcessEntry.SafetyLabel), "Safety", 140));
        portsGrid.Columns.Add(CreateTextColumn(nameof(PortProcessEntry.ExecutablePath), "Path", 320));
    }

    private void ConfigureLanguagePicker()
    {
        languageComboBox.DisplayMember = "Label";
        languageComboBox.ValueMember = "Value";
        languageComboBox.Items.Add(new LanguageOption("English", UiLanguage.English));
        languageComboBox.Items.Add(new LanguageOption("Espanol", UiLanguage.Spanish));
        languageComboBox.SelectedIndex = 1;
    }

    private void BuildFilterButtons()
    {
        filtersFlowPanel.Controls.Clear();
        _filterButtons.Clear();

        foreach (var filter in _devPortCatalog.GetFilters())
        {
            var button = new Button
            {
                AutoSize = true,
                AutoSizeMode = AutoSizeMode.GrowAndShrink,
                BackColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI Semibold", 9F, FontStyle.Bold, GraphicsUnit.Point, 0),
                ForeColor = Color.FromArgb(50, 58, 71),
                Margin = new Padding(0, 0, 10, 10),
                Padding = new Padding(12, 9, 12, 9),
                Tag = filter.Key,
                Text = filter.Label,
                UseVisualStyleBackColor = false
            };

            button.FlatAppearance.BorderColor = Color.FromArgb(215, 221, 228);
            button.FlatAppearance.MouseDownBackColor = Color.FromArgb(226, 236, 248);
            button.FlatAppearance.MouseOverBackColor = Color.FromArgb(238, 244, 250);

            if (filter.ProfileKey is not null)
            {
                button.Image = DevelopmentPortIconFactory.Get(filter.ProfileKey);
                button.ImageAlign = ContentAlignment.MiddleLeft;
                button.TextImageRelation = TextImageRelation.ImageBeforeText;
            }

            button.MouseEnter += FilterButton_MouseEnter;
            button.MouseLeave += FilterButton_MouseLeave;
            button.Click += FilterButton_Click;
            filtersFlowPanel.Controls.Add(button);
            _filterButtons[filter.Key] = button;
        }

        UpdateFilterButtonStyles();
    }

    private static DataGridViewTextBoxColumn CreateTextColumn(string propertyName, string title, int width)
    {
        return new DataGridViewTextBoxColumn
        {
            DataPropertyName = propertyName,
            HeaderText = title,
            Width = width,
            MinimumWidth = width,
            SortMode = DataGridViewColumnSortMode.Automatic
        };
    }

    private async Task RefreshEntriesAsync()
    {
        if (_isRefreshing)
        {
            return;
        }

        _isRefreshing = true;
        refreshButton.Enabled = false;
        statusLabel.Text = UiTextCatalog.Get(_language, UiTextKey.RefreshingPorts);

        try
        {
            var entries = await Task.Run(() => _portService.GetEntries());
            _allEntries = entries.ToList();
            ApplyFilters();
            statusLabel.Text = UiTextCatalog.Format(_language, UiTextKey.PortsLoaded, _allEntries.Count);
        }
        catch (Exception exception)
        {
            statusLabel.Text = UiTextCatalog.Get(_language, UiTextKey.RefreshFailed);
            MessageBox.Show(
                this,
                exception.Message,
                UiTextCatalog.Get(_language, UiTextKey.UnableToReadPortsTitle),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }
        finally
        {
            refreshButton.Enabled = true;
            _isRefreshing = false;
        }
    }

    private void ApplyFilters()
    {
        var query = searchTextBox.Text.Trim();
        IEnumerable<PortProcessEntry> filtered = _allEntries;

        if (!showProtectedCheckBox.Checked)
        {
            filtered = filtered.Where(entry => !entry.IsProtected);
        }

        filtered = _selectedFilter switch
        {
            DevelopmentPortCatalog.DevelopmentOnlyFilterKey => filtered.Where(entry => entry.IsDevelopmentPort),
            DevelopmentPortCatalog.AllFilterKey => filtered,
            _ => filtered.Where(entry => string.Equals(entry.DevProfileKey, _selectedFilter, StringComparison.OrdinalIgnoreCase))
        };

        if (!string.IsNullOrWhiteSpace(query))
        {
            filtered = filtered.Where(entry =>
                entry.Port.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                entry.ProcessId.ToString().Contains(query, StringComparison.OrdinalIgnoreCase) ||
                entry.ProcessName.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                entry.LocalAddress.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                entry.ExecutablePath.Contains(query, StringComparison.OrdinalIgnoreCase) ||
                entry.DevProfileLabel.Contains(query, StringComparison.OrdinalIgnoreCase));
        }

        var ordered = filtered
            .OrderByDescending(entry => entry.IsDevelopmentPort)
            .ThenBy(entry => entry.Port)
            .ThenBy(entry => entry.ProcessName)
            .ToList();

        _rowsSource.DataSource = ordered;
        ApplyRowStyles();
        UpdateSelectionDetails();
        resultCountLabel.Text = UiTextCatalog.Format(_language, UiTextKey.ShownCount, ordered.Count);
    }

    private void ApplyRowStyles()
    {
        foreach (DataGridViewRow row in portsGrid.Rows)
        {
            if (row.DataBoundItem is not PortProcessEntry entry)
            {
                continue;
            }

            if (entry.IsProtected)
            {
                row.DefaultCellStyle.BackColor = Color.FromArgb(246, 239, 236);
                row.DefaultCellStyle.ForeColor = Color.FromArgb(106, 84, 71);
                continue;
            }

            row.DefaultCellStyle.BackColor = entry.IsDevelopmentPort
                ? Color.FromArgb(242, 248, 255)
                : Color.White;

            row.DefaultCellStyle.ForeColor = Color.FromArgb(35, 35, 35);
        }
    }

    private void UpdateSelectionDetails()
    {
        var selected = GetSelectedEntry();

        if (selected is null)
        {
            selectedPortLabel.Text = UiTextCatalog.Get(_language, UiTextKey.NoPortSelected);
            selectedProcessLabel.Text = UiTextCatalog.Get(_language, UiTextKey.ChooseRowHint);
            selectedPathLabel.Text = $"{UiTextCatalog.Get(_language, UiTextKey.PathLabel)}: -";
            protectionLabel.Text = $"{UiTextCatalog.Get(_language, UiTextKey.ProtectionLabel)}: -";
            stopSelectedButton.Enabled = false;
            return;
        }

        selectedPortLabel.Text = $"{selected.Protocol} {selected.LocalAddress}:{selected.Port}";
        selectedProcessLabel.Text = $"{selected.ProcessName} (PID {selected.ProcessId})";
        selectedPathLabel.Text = $"{UiTextCatalog.Get(_language, UiTextKey.PathLabel)}: {selected.ExecutablePath}";
        protectionLabel.Text = $"{UiTextCatalog.Get(_language, UiTextKey.ProtectionLabel)}: {selected.ProtectionReason}";
        stopSelectedButton.Enabled = !selected.IsProtected;
    }

    private PortProcessEntry? GetSelectedEntry()
    {
        if (portsGrid.CurrentRow?.DataBoundItem is PortProcessEntry entry)
        {
            return entry;
        }

        return null;
    }

    private async Task StopSelectedProcessAsync()
    {
        var selected = GetSelectedEntry();
        if (selected is null)
        {
            return;
        }

        if (selected.IsProtected)
        {
            MessageBox.Show(
                this,
                selected.ProtectionReason,
                UiTextCatalog.Get(_language, UiTextKey.ProtectedProcessTitle),
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        var decision = MessageBox.Show(
            this,
            UiTextCatalog.Format(_language, UiTextKey.ConfirmStopBody, selected.ProcessName, selected.ProcessId, selected.Port),
            UiTextCatalog.Get(_language, UiTextKey.ConfirmStopTitle),
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Warning,
            MessageBoxDefaultButton.Button2);

        if (decision != DialogResult.Yes)
        {
            return;
        }

        try
        {
            using var process = Process.GetProcessById(selected.ProcessId);
            process.Kill();
            process.WaitForExit(3000);
            statusLabel.Text = UiTextCatalog.Format(_language, UiTextKey.StoppedProcess, selected.ProcessId);
        }
        catch (Exception exception)
        {
            MessageBox.Show(
                this,
                exception.Message,
                UiTextCatalog.Get(_language, UiTextKey.StopFailedTitle),
                MessageBoxButtons.OK,
                MessageBoxIcon.Error);
        }

        await RefreshEntriesAsync();
    }

    private void AutoRefreshCheckBox_CheckedChanged(object? sender, EventArgs eventArgs)
    {
        _refreshTimer.Enabled = autoRefreshCheckBox.Checked;
        UpdateAutoRefreshText();
    }

    private void LanguageComboBox_SelectedIndexChanged(object? sender, EventArgs eventArgs)
    {
        if (languageComboBox.SelectedItem is LanguageOption option)
        {
            _language = option.Value;
            ApplyLanguage();
            ApplyFilters();
        }
    }

    private void FilterButton_Click(object? sender, EventArgs eventArgs)
    {
        if (sender is not Button button || button.Tag is not string filterKey)
        {
            return;
        }

        _selectedFilter = filterKey;
        UpdateFilterButtonStyles();
        ApplyFilters();
    }

    private void FilterButton_MouseEnter(object? sender, EventArgs eventArgs)
    {
        if (sender is Button button)
        {
            ApplyFilterButtonTheme(button, string.Equals(button.Tag as string, _selectedFilter, StringComparison.OrdinalIgnoreCase), true);
        }
    }

    private void FilterButton_MouseLeave(object? sender, EventArgs eventArgs)
    {
        if (sender is Button button)
        {
            ApplyFilterButtonTheme(button, string.Equals(button.Tag as string, _selectedFilter, StringComparison.OrdinalIgnoreCase), false);
        }
    }

    private void UpdateFilterButtonStyles()
    {
        foreach (var pair in _filterButtons)
        {
            var isActive = string.Equals(pair.Key, _selectedFilter, StringComparison.OrdinalIgnoreCase);
            ApplyFilterButtonTheme(pair.Value, isActive, false);
        }
    }

    private static void ApplyFilterButtonTheme(Button button, bool isActive, bool isHovering)
    {
        button.BackColor = isActive
            ? (isHovering ? Color.FromArgb(36, 110, 201) : Color.FromArgb(28, 98, 186))
            : (isHovering ? Color.FromArgb(238, 244, 250) : Color.White);
        button.ForeColor = isActive ? Color.White : Color.FromArgb(50, 58, 71);
        button.FlatAppearance.BorderColor = isActive
            ? Color.FromArgb(28, 98, 186)
            : Color.FromArgb(215, 221, 228);
        button.FlatAppearance.MouseOverBackColor = isActive
            ? Color.FromArgb(36, 110, 201)
            : Color.FromArgb(238, 244, 250);
        button.FlatAppearance.MouseDownBackColor = isActive
            ? Color.FromArgb(24, 86, 164)
            : Color.FromArgb(226, 236, 248);
    }

    private void ApplyLanguage()
    {
        subtitleLabel.Text = UiTextCatalog.Get(_language, UiTextKey.Subtitle);
        refreshButton.Text = UiTextCatalog.Get(_language, UiTextKey.RefreshNow);
        searchLabel.Text = UiTextCatalog.Get(_language, UiTextKey.Search);
        searchTextBox.PlaceholderText = UiTextCatalog.Get(_language, UiTextKey.SearchPlaceholder);
        quickFiltersLabel.Text = UiTextCatalog.Get(_language, UiTextKey.QuickFilters);
        showProtectedCheckBox.Text = UiTextCatalog.Get(_language, UiTextKey.ShowProtected);
        stopSelectedButton.Text = UiTextCatalog.Get(_language, UiTextKey.StopSelectedProcess);
        Text = UiTextCatalog.Get(_language, UiTextKey.WindowTitle);

        portsGrid.Columns["DevIcon"]!.HeaderText = string.Empty;
        portsGrid.Columns[1].HeaderText = UiTextCatalog.Get(_language, UiTextKey.DevColumn);
        portsGrid.Columns[2].HeaderText = UiTextCatalog.Get(_language, UiTextKey.PortColumn);
        portsGrid.Columns[3].HeaderText = UiTextCatalog.Get(_language, UiTextKey.ProtocolColumn);
        portsGrid.Columns[4].HeaderText = UiTextCatalog.Get(_language, UiTextKey.AddressColumn);
        portsGrid.Columns[5].HeaderText = UiTextCatalog.Get(_language, UiTextKey.ProcessColumn);
        portsGrid.Columns[6].HeaderText = UiTextCatalog.Get(_language, UiTextKey.PidColumn);
        portsGrid.Columns[7].HeaderText = UiTextCatalog.Get(_language, UiTextKey.StateColumn);
        portsGrid.Columns[8].HeaderText = UiTextCatalog.Get(_language, UiTextKey.SafetyColumn);
        portsGrid.Columns[9].HeaderText = UiTextCatalog.Get(_language, UiTextKey.PathColumn);

        foreach (var filter in _devPortCatalog.GetFilters())
        {
            if (_filterButtons.TryGetValue(filter.Key, out var button))
            {
                button.Text = filter.GetLabel(_language);
            }
        }

        if (string.IsNullOrWhiteSpace(statusLabel.Text) ||
            statusLabel.Text == UiTextCatalog.Get(UiLanguage.English, UiTextKey.ReadyToScan) ||
            statusLabel.Text == UiTextCatalog.Get(UiLanguage.Spanish, UiTextKey.ReadyToScan))
        {
            statusLabel.Text = UiTextCatalog.Get(_language, UiTextKey.ReadyToScan);
        }

        UpdateAutoRefreshText();
        UpdateSelectionDetails();
    }

    private void UpdateAutoRefreshText()
    {
        autoRefreshCheckBox.Text = autoRefreshCheckBox.Checked
            ? UiTextCatalog.Get(_language, UiTextKey.AutoRefreshOn)
            : UiTextCatalog.Get(_language, UiTextKey.AutoRefreshOff);
    }

    private sealed record LanguageOption(string Label, UiLanguage Value)
    {
        public override string ToString() => Label;
    }
}
