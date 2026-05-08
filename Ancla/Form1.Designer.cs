namespace Ancla;

partial class Form1
{
    private System.ComponentModel.IContainer components = null;
    private Panel headerPanel = null!;
    private Label titleLabel = null!;
    private Label subtitleLabel = null!;
    private Button refreshButton = null!;
    private CheckBox autoRefreshCheckBox = null!;
    private ComboBox languageComboBox = null!;
    private Panel filterPanel = null!;
    private Label searchLabel = null!;
    private TextBox searchTextBox = null!;
    private Label resultCountLabel = null!;
    private CheckBox showProtectedCheckBox = null!;
    private Label quickFiltersLabel = null!;
    private FlowLayoutPanel filtersFlowPanel = null!;
    private DataGridView portsGrid = null!;
    private Panel footerPanel = null!;
    private Label selectedPortLabel = null!;
    private Label selectedProcessLabel = null!;
    private Label selectedPathLabel = null!;
    private Label protectionLabel = null!;
    private Button stopSelectedButton = null!;
    private StatusStrip statusStrip = null!;
    private ToolStripStatusLabel statusLabel = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing && (components != null))
        {
            components.Dispose();
        }

        base.Dispose(disposing);
    }

    #region Windows Form Designer generated code

    private void InitializeComponent()
    {
        components = new System.ComponentModel.Container();
        headerPanel = new Panel();
        languageComboBox = new ComboBox();
        autoRefreshCheckBox = new CheckBox();
        refreshButton = new Button();
        subtitleLabel = new Label();
        titleLabel = new Label();
        filterPanel = new Panel();
        filtersFlowPanel = new FlowLayoutPanel();
        quickFiltersLabel = new Label();
        showProtectedCheckBox = new CheckBox();
        resultCountLabel = new Label();
        searchTextBox = new TextBox();
        searchLabel = new Label();
        portsGrid = new DataGridView();
        footerPanel = new Panel();
        stopSelectedButton = new Button();
        protectionLabel = new Label();
        selectedPathLabel = new Label();
        selectedProcessLabel = new Label();
        selectedPortLabel = new Label();
        statusStrip = new StatusStrip();
        statusLabel = new ToolStripStatusLabel();
        headerPanel.SuspendLayout();
        filterPanel.SuspendLayout();
        ((System.ComponentModel.ISupportInitialize)portsGrid).BeginInit();
        footerPanel.SuspendLayout();
        statusStrip.SuspendLayout();
        SuspendLayout();
        //
        // headerPanel
        //
        headerPanel.BackColor = Color.FromArgb(24, 32, 45);
        headerPanel.Controls.Add(languageComboBox);
        headerPanel.Controls.Add(autoRefreshCheckBox);
        headerPanel.Controls.Add(refreshButton);
        headerPanel.Controls.Add(subtitleLabel);
        headerPanel.Controls.Add(titleLabel);
        headerPanel.Dock = DockStyle.Top;
        headerPanel.Location = new Point(0, 0);
        headerPanel.Name = "headerPanel";
        headerPanel.Padding = new Padding(20, 18, 20, 16);
        headerPanel.Size = new Size(1280, 96);
        headerPanel.TabIndex = 0;
        //
        // languageComboBox
        //
        languageComboBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        languageComboBox.DropDownStyle = ComboBoxStyle.DropDownList;
        languageComboBox.FlatStyle = FlatStyle.Flat;
        languageComboBox.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
        languageComboBox.FormattingEnabled = true;
        languageComboBox.Location = new Point(748, 33);
        languageComboBox.Name = "languageComboBox";
        languageComboBox.Size = new Size(121, 25);
        languageComboBox.TabIndex = 4;
        //
        // autoRefreshCheckBox
        //
        autoRefreshCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        autoRefreshCheckBox.AutoSize = true;
        autoRefreshCheckBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        autoRefreshCheckBox.ForeColor = Color.White;
        autoRefreshCheckBox.Location = new Point(889, 35);
        autoRefreshCheckBox.Name = "autoRefreshCheckBox";
        autoRefreshCheckBox.Size = new Size(173, 19);
        autoRefreshCheckBox.TabIndex = 3;
        autoRefreshCheckBox.Text = "Auto refresh every 5 seconds";
        autoRefreshCheckBox.UseVisualStyleBackColor = true;
        //
        // refreshButton
        //
        refreshButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        refreshButton.BackColor = Color.FromArgb(231, 91, 53);
        refreshButton.FlatAppearance.BorderSize = 0;
        refreshButton.FlatStyle = FlatStyle.Flat;
        refreshButton.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
        refreshButton.ForeColor = Color.White;
        refreshButton.Location = new Point(1110, 23);
        refreshButton.Name = "refreshButton";
        refreshButton.Size = new Size(150, 42);
        refreshButton.TabIndex = 2;
        refreshButton.Text = "Refresh now";
        refreshButton.UseVisualStyleBackColor = false;
        //
        // subtitleLabel
        //
        subtitleLabel.AutoSize = true;
        subtitleLabel.Font = new Font("Segoe UI", 9.75F, FontStyle.Regular, GraphicsUnit.Point, 0);
        subtitleLabel.ForeColor = Color.FromArgb(215, 222, 232);
        subtitleLabel.Location = new Point(22, 52);
        subtitleLabel.Name = "subtitleLabel";
        subtitleLabel.Size = new Size(432, 17);
        subtitleLabel.TabIndex = 1;
        subtitleLabel.Text = "See which apps own your local ports and stop only the safe ones.";
        //
        // titleLabel
        //
        titleLabel.AutoSize = true;
        titleLabel.Font = new Font("Segoe UI Semibold", 18F, FontStyle.Bold, GraphicsUnit.Point, 0);
        titleLabel.ForeColor = Color.White;
        titleLabel.Location = new Point(20, 14);
        titleLabel.Name = "titleLabel";
        titleLabel.Size = new Size(126, 32);
        titleLabel.TabIndex = 0;
        titleLabel.Text = "Ancla";
        //
        // filterPanel
        //
        filterPanel.BackColor = Color.FromArgb(244, 246, 249);
        filterPanel.Controls.Add(filtersFlowPanel);
        filterPanel.Controls.Add(quickFiltersLabel);
        filterPanel.Controls.Add(showProtectedCheckBox);
        filterPanel.Controls.Add(resultCountLabel);
        filterPanel.Controls.Add(searchTextBox);
        filterPanel.Controls.Add(searchLabel);
        filterPanel.Dock = DockStyle.Top;
        filterPanel.Location = new Point(0, 96);
        filterPanel.Name = "filterPanel";
        filterPanel.Padding = new Padding(20, 14, 20, 14);
        filterPanel.Size = new Size(1280, 132);
        filterPanel.TabIndex = 1;
        //
        // filtersFlowPanel
        //
        filtersFlowPanel.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        filtersFlowPanel.Location = new Point(151, 61);
        filtersFlowPanel.Name = "filtersFlowPanel";
        filtersFlowPanel.Size = new Size(1110, 46);
        filtersFlowPanel.TabIndex = 5;
        //
        // quickFiltersLabel
        //
        quickFiltersLabel.AutoSize = true;
        quickFiltersLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
        quickFiltersLabel.ForeColor = Color.FromArgb(41, 48, 60);
        quickFiltersLabel.Location = new Point(20, 74);
        quickFiltersLabel.Name = "quickFiltersLabel";
        quickFiltersLabel.Size = new Size(91, 19);
        quickFiltersLabel.TabIndex = 4;
        quickFiltersLabel.Text = "Quick filters";
        //
        // showProtectedCheckBox
        //
        showProtectedCheckBox.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        showProtectedCheckBox.AutoSize = true;
        showProtectedCheckBox.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        showProtectedCheckBox.ForeColor = Color.FromArgb(53, 60, 72);
        showProtectedCheckBox.Location = new Point(1081, 20);
        showProtectedCheckBox.Name = "showProtectedCheckBox";
        showProtectedCheckBox.Size = new Size(179, 19);
        showProtectedCheckBox.TabIndex = 3;
        showProtectedCheckBox.Text = "Show protected system ports";
        showProtectedCheckBox.UseVisualStyleBackColor = true;
        //
        // resultCountLabel
        //
        resultCountLabel.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        resultCountLabel.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
        resultCountLabel.ForeColor = Color.FromArgb(89, 97, 111);
        resultCountLabel.Location = new Point(1080, 49);
        resultCountLabel.Name = "resultCountLabel";
        resultCountLabel.Size = new Size(180, 19);
        resultCountLabel.TabIndex = 2;
        resultCountLabel.Text = "0 shown";
        resultCountLabel.TextAlign = ContentAlignment.MiddleRight;
        //
        // searchTextBox
        //
        searchTextBox.Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right;
        searchTextBox.BorderStyle = BorderStyle.FixedSingle;
        searchTextBox.Font = new Font("Segoe UI", 10F, FontStyle.Regular, GraphicsUnit.Point, 0);
        searchTextBox.Location = new Point(92, 18);
        searchTextBox.Name = "searchTextBox";
        searchTextBox.PlaceholderText = "Port, process, PID or path";
        searchTextBox.Size = new Size(972, 25);
        searchTextBox.TabIndex = 1;
        //
        // searchLabel
        //
        searchLabel.AutoSize = true;
        searchLabel.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
        searchLabel.ForeColor = Color.FromArgb(41, 48, 60);
        searchLabel.Location = new Point(20, 21);
        searchLabel.Name = "searchLabel";
        searchLabel.Size = new Size(55, 19);
        searchLabel.TabIndex = 0;
        searchLabel.Text = "Search";
        //
        // portsGrid
        //
        portsGrid.AllowUserToAddRows = false;
        portsGrid.AllowUserToDeleteRows = false;
        portsGrid.AllowUserToResizeRows = false;
        portsGrid.BackgroundColor = Color.White;
        portsGrid.BorderStyle = BorderStyle.None;
        portsGrid.ColumnHeadersHeight = 42;
        portsGrid.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
        portsGrid.Dock = DockStyle.Fill;
        portsGrid.EnableHeadersVisualStyles = false;
        portsGrid.Location = new Point(0, 228);
        portsGrid.MultiSelect = false;
        portsGrid.Name = "portsGrid";
        portsGrid.ReadOnly = true;
        portsGrid.RowHeadersVisible = false;
        portsGrid.RowTemplate.Height = 34;
        portsGrid.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        portsGrid.Size = new Size(1280, 400);
        portsGrid.TabIndex = 2;
        portsGrid.DefaultCellStyle = new DataGridViewCellStyle
        {
            SelectionBackColor = Color.FromArgb(214, 232, 246),
            SelectionForeColor = Color.FromArgb(18, 23, 31)
        };
        portsGrid.ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
        {
            BackColor = Color.FromArgb(238, 242, 247),
            ForeColor = Color.FromArgb(40, 46, 58),
            Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point, 0)
        };
        //
        // footerPanel
        //
        footerPanel.BackColor = Color.FromArgb(248, 249, 251);
        footerPanel.Controls.Add(stopSelectedButton);
        footerPanel.Controls.Add(protectionLabel);
        footerPanel.Controls.Add(selectedPathLabel);
        footerPanel.Controls.Add(selectedProcessLabel);
        footerPanel.Controls.Add(selectedPortLabel);
        footerPanel.Dock = DockStyle.Bottom;
        footerPanel.Location = new Point(0, 628);
        footerPanel.Name = "footerPanel";
        footerPanel.Padding = new Padding(20, 14, 20, 14);
        footerPanel.Size = new Size(1280, 104);
        footerPanel.TabIndex = 3;
        //
        // stopSelectedButton
        //
        stopSelectedButton.Anchor = AnchorStyles.Top | AnchorStyles.Right;
        stopSelectedButton.BackColor = Color.FromArgb(212, 65, 58);
        stopSelectedButton.Enabled = false;
        stopSelectedButton.FlatAppearance.BorderSize = 0;
        stopSelectedButton.FlatStyle = FlatStyle.Flat;
        stopSelectedButton.Font = new Font("Segoe UI Semibold", 10F, FontStyle.Bold, GraphicsUnit.Point, 0);
        stopSelectedButton.ForeColor = Color.White;
        stopSelectedButton.Location = new Point(1069, 28);
        stopSelectedButton.Name = "stopSelectedButton";
        stopSelectedButton.Size = new Size(191, 46);
        stopSelectedButton.TabIndex = 4;
        stopSelectedButton.Text = "Stop selected process";
        stopSelectedButton.UseVisualStyleBackColor = false;
        //
        // protectionLabel
        //
        protectionLabel.AutoEllipsis = true;
        protectionLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        protectionLabel.ForeColor = Color.FromArgb(88, 95, 108);
        protectionLabel.Location = new Point(20, 81);
        protectionLabel.Name = "protectionLabel";
        protectionLabel.Size = new Size(870, 17);
        protectionLabel.TabIndex = 3;
        protectionLabel.Text = "Protection: -";
        //
        // selectedPathLabel
        //
        selectedPathLabel.AutoEllipsis = true;
        selectedPathLabel.Font = new Font("Segoe UI", 9F, FontStyle.Regular, GraphicsUnit.Point, 0);
        selectedPathLabel.ForeColor = Color.FromArgb(88, 95, 108);
        selectedPathLabel.Location = new Point(20, 62);
        selectedPathLabel.Name = "selectedPathLabel";
        selectedPathLabel.Size = new Size(870, 17);
        selectedPathLabel.TabIndex = 2;
        selectedPathLabel.Text = "Path: -";
        //
        // selectedProcessLabel
        //
        selectedProcessLabel.AutoEllipsis = true;
        selectedProcessLabel.Font = new Font("Segoe UI", 9.5F, FontStyle.Regular, GraphicsUnit.Point, 0);
        selectedProcessLabel.ForeColor = Color.FromArgb(66, 74, 87);
        selectedProcessLabel.Location = new Point(20, 40);
        selectedProcessLabel.Name = "selectedProcessLabel";
        selectedProcessLabel.Size = new Size(870, 18);
        selectedProcessLabel.TabIndex = 1;
        selectedProcessLabel.Text = "Choose a row to inspect a process";
        //
        // selectedPortLabel
        //
        selectedPortLabel.AutoEllipsis = true;
        selectedPortLabel.Font = new Font("Segoe UI Semibold", 11F, FontStyle.Bold, GraphicsUnit.Point, 0);
        selectedPortLabel.ForeColor = Color.FromArgb(31, 37, 46);
        selectedPortLabel.Location = new Point(20, 14);
        selectedPortLabel.Name = "selectedPortLabel";
        selectedPortLabel.Size = new Size(870, 22);
        selectedPortLabel.TabIndex = 0;
        selectedPortLabel.Text = "No port selected";
        //
        // statusStrip
        //
        statusStrip.Items.AddRange(new ToolStripItem[] { statusLabel });
        statusStrip.Location = new Point(0, 732);
        statusStrip.Name = "statusStrip";
        statusStrip.Size = new Size(1280, 22);
        statusStrip.TabIndex = 4;
        //
        // statusLabel
        //
        statusLabel.Name = "statusLabel";
        statusLabel.Size = new Size(80, 17);
        statusLabel.Text = "Ready to scan";
        //
        // Form1
        //
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        BackColor = Color.White;
        ClientSize = new Size(1280, 754);
        Controls.Add(portsGrid);
        Controls.Add(footerPanel);
        Controls.Add(statusStrip);
        Controls.Add(filterPanel);
        Controls.Add(headerPanel);
        MinimumSize = new Size(1180, 730);
        Name = "Form1";
        StartPosition = FormStartPosition.CenterScreen;
        Text = "Ancla";
        headerPanel.ResumeLayout(false);
        headerPanel.PerformLayout();
        filterPanel.ResumeLayout(false);
        filterPanel.PerformLayout();
        ((System.ComponentModel.ISupportInitialize)portsGrid).EndInit();
        footerPanel.ResumeLayout(false);
        statusStrip.ResumeLayout(false);
        statusStrip.PerformLayout();
        ResumeLayout(false);
        PerformLayout();
    }

    #endregion
}
