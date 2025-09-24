#nullable enable
using System.ComponentModel;
using System.Drawing;
using System.Windows.Forms;

namespace OtakAgent.App.Forms;

partial class SettingsForm
{
    private IContainer? components = null;
    private TabControl _tabControl = null!;
    private TabPage _generalTab = null!;
    private TabPage _presetsTab = null!;

    // General tab controls
    private CheckBox _englishCheckBox = null!;
    private CheckBox _expandedTextboxCheckBox = null!;
    private CheckBox _enablePersonalityCheckBox = null!;
    private CheckBox _autoCopyCheckBox = null!;
    private CheckBox _historyCheckBox = null!;
    private CheckBox _webSearchCheckBox = null!;
    private TextBox _hostTextBox = null!;
    private TextBox _endpointTextBox = null!;
    private TextBox _apiKeyTextBox = null!;
    private TextBox _modelTextBox = null!;
    private TextBox _systemPromptTextBox = null!;
    private TextBox _personalityOverrideTextBox = null!;

    // Presets tab controls
    private ListBox _presetsListBox = null!;
    private TextBox _presetNameTextBox = null!;
    private TextBox _presetPromptTextBox = null!;
    private Button _addPresetButton = null!;
    private Button _updatePresetButton = null!;
    private Button _deletePresetButton = null!;
    private Button _applyPresetButton = null!;

    // Form buttons
    private Button _saveButton = null!;
    private Button _cancelButton = null!;
    private Button _resetDefaultsButton = null!;

    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            components?.Dispose();
        }
        base.Dispose(disposing);
    }

    private void InitializeComponent()
    {
        components = new Container();

        // Tab Control
        _tabControl = new TabControl();
        _generalTab = new TabPage();
        _presetsTab = new TabPage();

        // General tab controls
        _englishCheckBox = new CheckBox();
        _expandedTextboxCheckBox = new CheckBox();
        _enablePersonalityCheckBox = new CheckBox();
        _autoCopyCheckBox = new CheckBox();
        _historyCheckBox = new CheckBox();
        _webSearchCheckBox = new CheckBox();
        _hostTextBox = new TextBox();
        _endpointTextBox = new TextBox();
        _apiKeyTextBox = new TextBox();
        _modelTextBox = new TextBox();
        _systemPromptTextBox = new TextBox();
        _personalityOverrideTextBox = new TextBox();

        // Presets tab controls
        _presetsListBox = new ListBox();
        _presetNameTextBox = new TextBox();
        _presetPromptTextBox = new TextBox();
        _addPresetButton = new Button();
        _updatePresetButton = new Button();
        _deletePresetButton = new Button();
        _applyPresetButton = new Button();

        // Form buttons
        _resetDefaultsButton = new Button();
        _saveButton = new Button();
        _cancelButton = new Button();

        SuspendLayout();

        // Tab Control
        _tabControl.Location = new Point(12, 12);
        _tabControl.Name = "_tabControl";
        _tabControl.Size = new Size(360, 500);
        _tabControl.TabIndex = 0;

        // General Tab
        _generalTab.Text = "General";
        _generalTab.UseVisualStyleBackColor = true;
        _generalTab.Padding = new Padding(3);

        // Add controls to General tab
        _englishCheckBox.AutoSize = true;
        _englishCheckBox.Location = new Point(16, 20);
        _englishCheckBox.Text = "English UI";
        _englishCheckBox.TabIndex = 0;
        _generalTab.Controls.Add(_englishCheckBox);

        _expandedTextboxCheckBox.AutoSize = true;
        _expandedTextboxCheckBox.Location = new Point(16, 44);
        _expandedTextboxCheckBox.Text = "Expanded Textbox";
        _expandedTextboxCheckBox.TabIndex = 1;
        _generalTab.Controls.Add(_expandedTextboxCheckBox);

        _enablePersonalityCheckBox.AutoSize = true;
        _enablePersonalityCheckBox.Location = new Point(16, 68);
        _enablePersonalityCheckBox.Text = "Enable Personality";
        _enablePersonalityCheckBox.TabIndex = 2;
        _generalTab.Controls.Add(_enablePersonalityCheckBox);

        _autoCopyCheckBox.AutoSize = true;
        _autoCopyCheckBox.Location = new Point(16, 92);
        _autoCopyCheckBox.Text = "Auto-copy to Clipboard";
        _autoCopyCheckBox.TabIndex = 3;
        _generalTab.Controls.Add(_autoCopyCheckBox);

        _historyCheckBox.AutoSize = true;
        _historyCheckBox.Location = new Point(16, 116);
        _historyCheckBox.Text = "Use Conversation History";
        _historyCheckBox.TabIndex = 4;
        _generalTab.Controls.Add(_historyCheckBox);


        _webSearchCheckBox.AutoSize = true;
        _webSearchCheckBox.Location = new Point(16, 164);
        _webSearchCheckBox.Text = "Enable Web Search";
        _webSearchCheckBox.TabIndex = 7;
        _generalTab.Controls.Add(_webSearchCheckBox);

        var hostLabel = new Label();
        hostLabel.AutoSize = true;
        hostLabel.Location = new Point(16, 196);
        hostLabel.Text = "API Host:";
        _generalTab.Controls.Add(hostLabel);

        _hostTextBox.Location = new Point(16, 216);
        _hostTextBox.Size = new Size(320, 23);
        _hostTextBox.TabIndex = 8;
        _generalTab.Controls.Add(_hostTextBox);

        var endpointLabel = new Label();
        endpointLabel.AutoSize = true;
        endpointLabel.Location = new Point(16, 244);
        endpointLabel.Text = "Endpoint:";
        _generalTab.Controls.Add(endpointLabel);

        _endpointTextBox.Location = new Point(16, 264);
        _endpointTextBox.Size = new Size(320, 23);
        _endpointTextBox.TabIndex = 9;
        _generalTab.Controls.Add(_endpointTextBox);

        var apiKeyLabel = new Label();
        apiKeyLabel.AutoSize = true;
        apiKeyLabel.Location = new Point(16, 292);
        apiKeyLabel.Text = "API Key:";
        _generalTab.Controls.Add(apiKeyLabel);

        _apiKeyTextBox.Location = new Point(16, 312);
        _apiKeyTextBox.PasswordChar = '*';
        _apiKeyTextBox.Size = new Size(320, 23);
        _apiKeyTextBox.TabIndex = 10;
        _generalTab.Controls.Add(_apiKeyTextBox);

        var modelLabel = new Label();
        modelLabel.AutoSize = true;
        modelLabel.Location = new Point(16, 340);
        modelLabel.Text = "Model:";
        _generalTab.Controls.Add(modelLabel);

        _modelTextBox.Location = new Point(16, 360);
        _modelTextBox.Size = new Size(320, 23);
        _modelTextBox.TabIndex = 11;
        _generalTab.Controls.Add(_modelTextBox);

        var systemPromptLabel = new Label();
        systemPromptLabel.AutoSize = true;
        systemPromptLabel.Location = new Point(16, 388);
        systemPromptLabel.Text = "System Prompt:";
        _generalTab.Controls.Add(systemPromptLabel);

        _systemPromptTextBox.Location = new Point(16, 408);
        _systemPromptTextBox.Multiline = true;
        _systemPromptTextBox.ScrollBars = ScrollBars.Vertical;
        _systemPromptTextBox.Size = new Size(320, 60);
        _systemPromptTextBox.TabIndex = 12;
        _generalTab.Controls.Add(_systemPromptTextBox);

        // Personality override (hidden, small textbox)
        _personalityOverrideTextBox.Location = new Point(300, 20);
        _personalityOverrideTextBox.Size = new Size(10, 23);
        _personalityOverrideTextBox.Visible = false;
        _generalTab.Controls.Add(_personalityOverrideTextBox);

        // Presets Tab
        _presetsTab.Text = "Prompt Presets";
        _presetsTab.UseVisualStyleBackColor = true;
        _presetsTab.Padding = new Padding(3);

        // Presets list
        _presetsListBox.Location = new Point(16, 20);
        _presetsListBox.Size = new Size(320, 120);
        _presetsListBox.TabIndex = 0;
        _presetsTab.Controls.Add(_presetsListBox);

        // Preset name
        var presetNameLabel = new Label();
        presetNameLabel.AutoSize = true;
        presetNameLabel.Location = new Point(16, 150);
        presetNameLabel.Text = "Preset Name:";
        _presetsTab.Controls.Add(presetNameLabel);

        _presetNameTextBox.Location = new Point(16, 170);
        _presetNameTextBox.Size = new Size(320, 23);
        _presetNameTextBox.TabIndex = 1;
        _presetsTab.Controls.Add(_presetNameTextBox);

        // Preset prompt
        var presetPromptLabel = new Label();
        presetPromptLabel.AutoSize = true;
        presetPromptLabel.Location = new Point(16, 200);
        presetPromptLabel.Text = "Prompt:";
        _presetsTab.Controls.Add(presetPromptLabel);

        _presetPromptTextBox.Location = new Point(16, 220);
        _presetPromptTextBox.Multiline = true;
        _presetPromptTextBox.ScrollBars = ScrollBars.Vertical;
        _presetPromptTextBox.Size = new Size(320, 180);
        _presetPromptTextBox.TabIndex = 2;
        _presetsTab.Controls.Add(_presetPromptTextBox);

        // Preset buttons
        _addPresetButton.Location = new Point(16, 410);
        _addPresetButton.Size = new Size(75, 25);
        _addPresetButton.Text = "Add";
        _addPresetButton.TabIndex = 3;
        _addPresetButton.UseVisualStyleBackColor = true;
        _presetsTab.Controls.Add(_addPresetButton);

        _updatePresetButton.Location = new Point(96, 410);
        _updatePresetButton.Size = new Size(75, 25);
        _updatePresetButton.Text = "Update";
        _updatePresetButton.TabIndex = 4;
        _updatePresetButton.UseVisualStyleBackColor = true;
        _presetsTab.Controls.Add(_updatePresetButton);

        _deletePresetButton.Location = new Point(176, 410);
        _deletePresetButton.Size = new Size(75, 25);
        _deletePresetButton.Text = "Delete";
        _deletePresetButton.TabIndex = 5;
        _deletePresetButton.UseVisualStyleBackColor = true;
        _presetsTab.Controls.Add(_deletePresetButton);

        _applyPresetButton.Location = new Point(261, 410);
        _applyPresetButton.Size = new Size(75, 25);
        _applyPresetButton.Text = "Apply";
        _applyPresetButton.TabIndex = 6;
        _applyPresetButton.UseVisualStyleBackColor = true;
        _presetsTab.Controls.Add(_applyPresetButton);

        // Add tabs to control
        _tabControl.TabPages.Add(_generalTab);
        _tabControl.TabPages.Add(_presetsTab);

        // Form buttons
        _resetDefaultsButton.Location = new Point(16, 522);
        _resetDefaultsButton.Name = "_resetDefaultsButton";
        _resetDefaultsButton.Size = new Size(120, 32);
        _resetDefaultsButton.TabIndex = 13;
        _resetDefaultsButton.Text = "Reset to Defaults";
        _resetDefaultsButton.UseVisualStyleBackColor = true;

        _saveButton.Location = new Point(198, 522);
        _saveButton.Name = "_saveButton";
        _saveButton.Size = new Size(80, 32);
        _saveButton.TabIndex = 14;
        _saveButton.Text = "Save";
        _saveButton.UseVisualStyleBackColor = true;

        _cancelButton.Location = new Point(280, 522);
        _cancelButton.Name = "_cancelButton";
        _cancelButton.Size = new Size(80, 32);
        _cancelButton.TabIndex = 15;
        _cancelButton.Text = "Cancel";
        _cancelButton.UseVisualStyleBackColor = true;

        // Form
        AutoScaleDimensions = new SizeF(7F, 15F);
        AutoScaleMode = AutoScaleMode.Font;
        ClientSize = new Size(384, 566);
        Controls.Add(_tabControl);
        Controls.Add(_resetDefaultsButton);
        Controls.Add(_saveButton);
        Controls.Add(_cancelButton);
        FormBorderStyle = FormBorderStyle.FixedDialog;
        MaximizeBox = false;
        MinimizeBox = false;
        Name = "SettingsForm";
        ShowInTaskbar = false;
        StartPosition = FormStartPosition.CenterParent;
        Text = "Settings";
        ResumeLayout(false);
    }
}