using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Forms;
using OtakAgent.Core.Configuration;
using OtakAgent.Core.Validation;
using OtakAgent.Core.Security;

namespace OtakAgent.App.Forms;

public partial class SettingsForm : Form
{
    private readonly SettingsService _settingsService;
    private readonly AgentTalkSettings _workingCopy;
    private readonly List<SystemPromptPreset> _presets = new();
    private SystemPromptPreset? _selectedPreset;

    public SettingsForm(SettingsService settingsService, AgentTalkSettings currentSettings)
    {
        _settingsService = settingsService;
        _workingCopy = CloneSettings(currentSettings);

        InitializeComponent();
        Load += SettingsForm_Load;
        _saveButton.Click += async (_, _) => await SaveAsync().ConfigureAwait(false);
        _cancelButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        _resetDefaultsButton.Click += (_, _) => ResetToDefaults();
        _enablePersonalityCheckBox.CheckedChanged += (_, _) => UpdatePersonalityEditors();
        _englishCheckBox.CheckedChanged += (_, _) =>
        {
            ApplyLocalization();
            LoadPresets();  // Reload presets with new language
        };

        // Preset management event handlers
        _addPresetButton.Click += AddPresetButton_Click;
        _updatePresetButton.Click += UpdatePresetButton_Click;
        _deletePresetButton.Click += DeletePresetButton_Click;
        _applyPresetButton.Click += ApplyPresetButton_Click;
        _presetsListBox.SelectedIndexChanged += PresetsListBox_SelectedIndexChanged;
    }

    public AgentTalkSettings UpdatedSettings { get; private set; } = AgentTalkSettings.CreateDefault();

    private void SettingsForm_Load(object? sender, EventArgs e)
    {
        LoadPresets();
        BindSettingsToControls();
        ApplyLocalization();
    }

    private void LoadPresets()
    {
        _presets.Clear();

        // Load built-in presets
        _presets.AddRange(SystemPromptPreset.GetBuiltInPresets(_workingCopy.English));

        // Load user presets
        if (_workingCopy.SystemPromptPresets != null && _workingCopy.SystemPromptPresets.Count > 0)
        {
            _presets.AddRange(_workingCopy.SystemPromptPresets.Where(p => !p.IsBuiltIn));
        }

        RefreshPresetsListBox();
    }

    private void RefreshPresetsListBox()
    {
        _presetsListBox.Items.Clear();
        foreach (var preset in _presets)
        {
            var displayName = preset.IsBuiltIn ? $"[Built-in] {preset.Name}" : preset.Name;
            _presetsListBox.Items.Add(displayName);
        }
    }

    private void PresetsListBox_SelectedIndexChanged(object? sender, EventArgs e)
    {
        if (_presetsListBox.SelectedIndex >= 0 && _presetsListBox.SelectedIndex < _presets.Count)
        {
            _selectedPreset = _presets[_presetsListBox.SelectedIndex];
            _presetNameTextBox.Text = _selectedPreset.Name;
            _presetPromptTextBox.Text = _selectedPreset.Prompt;

            // Disable editing for built-in presets
            _presetNameTextBox.Enabled = !_selectedPreset.IsBuiltIn;
            _presetPromptTextBox.Enabled = !_selectedPreset.IsBuiltIn;
            _updatePresetButton.Enabled = !_selectedPreset.IsBuiltIn;
            _deletePresetButton.Enabled = !_selectedPreset.IsBuiltIn;
        }
    }

    private void AddPresetButton_Click(object? sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(_presetNameTextBox.Text))
        {
            MessageBox.Show(this, "Please enter a preset name.", "otak-agent", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        var newPreset = new SystemPromptPreset
        {
            Name = _presetNameTextBox.Text.Trim(),
            Prompt = _presetPromptTextBox.Text.Trim(),
            IsBuiltIn = false
        };

        _presets.Add(newPreset);
        RefreshPresetsListBox();
        _presetsListBox.SelectedIndex = _presets.Count - 1;
    }

    private void UpdatePresetButton_Click(object? sender, EventArgs e)
    {
        if (_selectedPreset == null || _selectedPreset.IsBuiltIn)
            return;

        if (string.IsNullOrWhiteSpace(_presetNameTextBox.Text))
        {
            MessageBox.Show(this, "Please enter a preset name.", "otak-agent", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        _selectedPreset.Name = _presetNameTextBox.Text.Trim();
        _selectedPreset.Prompt = _presetPromptTextBox.Text.Trim();
        _selectedPreset.UpdatedAt = DateTime.UtcNow;

        var selectedIndex = _presetsListBox.SelectedIndex;
        RefreshPresetsListBox();
        _presetsListBox.SelectedIndex = selectedIndex;
    }

    private void DeletePresetButton_Click(object? sender, EventArgs e)
    {
        if (_selectedPreset == null || _selectedPreset.IsBuiltIn)
            return;

        var result = MessageBox.Show(this,
            $"Are you sure you want to delete the preset '{_selectedPreset.Name}'?",
            "otak-agent",
            MessageBoxButtons.YesNo,
            MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            _presets.Remove(_selectedPreset);
            RefreshPresetsListBox();
            _selectedPreset = null;
            _presetNameTextBox.Clear();
            _presetPromptTextBox.Clear();
        }
    }

    private void ApplyPresetButton_Click(object? sender, EventArgs e)
    {
        if (_selectedPreset == null)
            return;

        _systemPromptTextBox.Text = _selectedPreset.Prompt;
        _workingCopy.SelectedPresetId = _selectedPreset.Id;

        // Switch to General tab to show the applied prompt
        _tabControl.SelectedTab = _generalTab;
    }

    private void BindSettingsToControls()
    {
        _englishCheckBox.Checked = _workingCopy.English;
        _expandedTextboxCheckBox.Checked = _workingCopy.ExpandedTextbox;
        _enablePersonalityCheckBox.Checked = _workingCopy.EnablePersonality;
        _autoCopyCheckBox.Checked = _workingCopy.AutoCopyToClipboard;
        _historyCheckBox.Checked = _workingCopy.UseConversationHistory;
        _hotkeyCheckBox.Checked = _workingCopy.ClipboardHotkeyEnabled;
        _webSearchCheckBox.Checked = _workingCopy.EnableWebSearch;
        _hotkeyIntervalNumeric.Value = Math.Clamp(_workingCopy.ClipboardHotkeyIntervalMs, 100, 2000);
        _hostTextBox.Text = _workingCopy.Host;
        _endpointTextBox.Text = _workingCopy.Endpoint;
        _apiKeyTextBox.Text = _workingCopy.ApiKey;
        _modelTextBox.Text = _workingCopy.Model;
        _personalityOverrideTextBox.Text = _workingCopy.PersonalityOverride;
        _systemPromptTextBox.Text = _workingCopy.SystemPrompt;
        UpdatePersonalityEditors();
    }

    private void ApplyLocalization()
    {
        if (_englishCheckBox.Checked)
        {
            Text = "Settings";
            _generalTab.Text = "General";
            _presetsTab.Text = "Prompt Presets";
            _englishCheckBox.Text = "Use English UI";
            _expandedTextboxCheckBox.Text = "Expanded input text area";
            _enablePersonalityCheckBox.Text = "Enable Clippy/Kairu persona";
            _autoCopyCheckBox.Text = "Auto-copy responses to clipboard";
            _historyCheckBox.Text = "Keep conversation history";
            _hotkeyCheckBox.Text = "Enable double Ctrl+C clipboard hotkey";
            _webSearchCheckBox.Text = "Enable Web Search (for compatible models)";
            _hostTextBox.PlaceholderText = "Host (e.g. api.openai.com)";
            _endpointTextBox.PlaceholderText = "Endpoint (e.g. /v1/chat/completions)";
            _apiKeyTextBox.PlaceholderText = "API Key";
            _modelTextBox.PlaceholderText = "Model (e.g. gpt-4o-mini)";
            _personalityOverrideTextBox.PlaceholderText = "Persona override (optional)";
            _systemPromptTextBox.PlaceholderText = "Additional system prompt";
            _addPresetButton.Text = "Add";
            _updatePresetButton.Text = "Update";
            _deletePresetButton.Text = "Delete";
            _applyPresetButton.Text = "Apply";
            _saveButton.Text = "Save";
            _cancelButton.Text = "Cancel";
            _resetDefaultsButton.Text = "Reset to Defaults";
        }
        else
        {
            Text = "設定";
            _generalTab.Text = "一般";
            _presetsTab.Text = "プロンプトプリセット";
            _englishCheckBox.Text = "英語 UI を使用";
            _expandedTextboxCheckBox.Text = "入力欄を拡大";
            _enablePersonalityCheckBox.Text = "キャラクター人格を有効化";
            _autoCopyCheckBox.Text = "返信を自動的にコピー";
            _historyCheckBox.Text = "会話履歴を保持";
            _hotkeyCheckBox.Text = "Ctrl+C 2 回でクリップボード送信";
            _webSearchCheckBox.Text = "Web 検索を有効化 (対応モデルのみ)";
            _hostTextBox.PlaceholderText = "ホスト (例: api.openai.com)";
            _endpointTextBox.PlaceholderText = "エンドポイント (例: /v1/chat/completions)";
            _apiKeyTextBox.PlaceholderText = "API キー";
            _modelTextBox.PlaceholderText = "モデル名";
            _personalityOverrideTextBox.PlaceholderText = "人格プロンプト (任意)";
            _systemPromptTextBox.PlaceholderText = "追加のシステムプロンプト";
            _addPresetButton.Text = "追加";
            _updatePresetButton.Text = "更新";
            _deletePresetButton.Text = "削除";
            _applyPresetButton.Text = "適用";
            _saveButton.Text = "保存";
            _cancelButton.Text = "キャンセル";
            _resetDefaultsButton.Text = "デフォルトに戻す";
        }
    }

    private async Task SaveAsync()
    {
        _workingCopy.English = _englishCheckBox.Checked;
        _workingCopy.ExpandedTextbox = _expandedTextboxCheckBox.Checked;
        _workingCopy.EnablePersonality = _enablePersonalityCheckBox.Checked;
        _workingCopy.AutoCopyToClipboard = _autoCopyCheckBox.Checked;
        _workingCopy.UseConversationHistory = _historyCheckBox.Checked;
        _workingCopy.ClipboardHotkeyEnabled = _hotkeyCheckBox.Checked;
        _workingCopy.EnableWebSearch = _webSearchCheckBox.Checked;
        _workingCopy.ClipboardHotkeyIntervalMs = (int)_hotkeyIntervalNumeric.Value;
        _workingCopy.Host = _hostTextBox.Text.Trim();
        _workingCopy.Endpoint = _endpointTextBox.Text.Trim();
        _workingCopy.ApiKey = _apiKeyTextBox.Text.Trim();
        _workingCopy.Model = _modelTextBox.Text.Trim();
        _workingCopy.PersonalityOverride = _personalityOverrideTextBox.Text.Trim();
        _workingCopy.SystemPrompt = _systemPromptTextBox.Text.Trim();

        // Save user presets
        _workingCopy.SystemPromptPresets = _presets.Where(p => !p.IsBuiltIn).ToList();

        var validationResult = SettingsValidator.ValidateSettings(_workingCopy);
        var errors = validationResult.Errors;
        if (!validationResult.IsValid)
        {
            var message = "Please fix the following errors:\n\n" + string.Join("\n", errors.Select(e => $"• {e}"));
            MessageBox.Show(this, message, "otak-agent", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        try
        {
            await _settingsService.SaveAsync(_workingCopy).ConfigureAwait(false);
            UpdatedSettings = _workingCopy;
            DialogResult = DialogResult.OK;

            if (InvokeRequired)
            {
                BeginInvoke(() => Close());
            }
            else
            {
                Close();
            }
        }
        catch (Exception ex)
        {
            MessageBox.Show(this, $"Failed to save settings: {ex.Message}", "otak-agent", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdatePersonalityEditors()
    {
        bool enablePersonality = _enablePersonalityCheckBox.Checked;
        // The personality override textbox is now hidden in the tab layout
        // Users can still edit system prompt directly
    }

    private void ResetToDefaults()
    {
        var result = MessageBox.Show(this, "Are you sure you want to reset all settings to their default values?",
            "otak-agent", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

        if (result == DialogResult.Yes)
        {
            var defaults = AgentTalkSettings.CreateDefault();
            CopySettings(defaults, _workingCopy);
            LoadPresets();
            BindSettingsToControls();
        }
    }

    private static AgentTalkSettings CloneSettings(AgentTalkSettings source)
    {
        var clone = AgentTalkSettings.CreateDefault();
        CopySettings(source, clone);
        return clone;
    }

    private static void CopySettings(AgentTalkSettings source, AgentTalkSettings target)
    {
        target.English = source.English;
        target.ExpandedTextbox = source.ExpandedTextbox;
        target.EnablePersonality = source.EnablePersonality;
        target.AutoCopyToClipboard = source.AutoCopyToClipboard;
        target.UseConversationHistory = source.UseConversationHistory;
        target.ClipboardHotkeyEnabled = source.ClipboardHotkeyEnabled;
        target.EnableWebSearch = source.EnableWebSearch;
        target.ClipboardHotkeyIntervalMs = source.ClipboardHotkeyIntervalMs;
        target.Host = source.Host;
        target.Endpoint = source.Endpoint;
        target.ApiKey = source.ApiKey;
        target.Model = source.Model;
        target.PersonalityOverride = source.PersonalityOverride;
        target.SystemPrompt = source.SystemPrompt;
        target.LastUserMessage = source.LastUserMessage;
        target.SystemPromptPresets = source.SystemPromptPresets?.ToList() ?? new List<SystemPromptPreset>();
        target.SelectedPresetId = source.SelectedPresetId;
    }
}