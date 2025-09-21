using System.Threading.Tasks;
using System.Windows.Forms;
using OtakAgent.Core.Configuration;

namespace OtakAgent.App.Forms;

public partial class SettingsForm : Form
{
    private readonly SettingsService _settingsService;
    private readonly AgentTalkSettings _workingCopy;

    public SettingsForm(SettingsService settingsService, AgentTalkSettings currentSettings)
    {
        _settingsService = settingsService;
        _workingCopy = CloneSettings(currentSettings);

        InitializeComponent();
        Load += SettingsForm_Load;
        _saveButton.Click += async (_, _) => await SaveAsync().ConfigureAwait(false);
        _cancelButton.Click += (_, _) => { DialogResult = DialogResult.Cancel; Close(); };
        _enablePersonalityCheckBox.CheckedChanged += (_, _) => UpdatePersonalityEditors();
        _englishCheckBox.CheckedChanged += (_, _) => ApplyLocalization();
    }

    public AgentTalkSettings UpdatedSettings { get; private set; } = AgentTalkSettings.CreateDefault();

    private void SettingsForm_Load(object? sender, EventArgs e)
    {
        BindSettingsToControls();
        ApplyLocalization();
    }

    private void BindSettingsToControls()
    {
        _englishCheckBox.Checked = _workingCopy.English;
        _expandedTextboxCheckBox.Checked = _workingCopy.ExpandedTextbox;
        _enablePersonalityCheckBox.Checked = _workingCopy.EnablePersonality;
        _autoCopyCheckBox.Checked = _workingCopy.AutoCopyToClipboard;
        _historyCheckBox.Checked = _workingCopy.UseConversationHistory;
        _hotkeyCheckBox.Checked = _workingCopy.ClipboardHotkeyEnabled;
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
            _englishCheckBox.Text = "Use English UI";
            _expandedTextboxCheckBox.Text = "Expanded input text area";
            _enablePersonalityCheckBox.Text = "Enable Clippy/Kairu persona";
            _autoCopyCheckBox.Text = "Auto-copy responses to clipboard";
            _historyCheckBox.Text = "Keep conversation history";
            _hotkeyCheckBox.Text = "Enable double Ctrl+C clipboard hotkey";
            _hostTextBox.PlaceholderText = "Host (e.g. api.openai.com)";
            _endpointTextBox.PlaceholderText = "Endpoint (e.g. /v1/chat/completions)";
            _apiKeyTextBox.PlaceholderText = "API Key";
            _modelTextBox.PlaceholderText = "Model (e.g. gpt-4o-mini)";
            _personalityOverrideTextBox.PlaceholderText = "Persona override (optional)";
            _systemPromptTextBox.PlaceholderText = "Additional system prompt";
            _saveButton.Text = "Save";
            _cancelButton.Text = "Cancel";
        }
        else
        {
            Text = "設定";
            _englishCheckBox.Text = "英語 UI を使用";
            _expandedTextboxCheckBox.Text = "入力欄を拡大";
            _enablePersonalityCheckBox.Text = "キャラクター人格を有効化";
            _autoCopyCheckBox.Text = "返信を自動的にコピー";
            _historyCheckBox.Text = "会話履歴を保持";
            _hotkeyCheckBox.Text = "Ctrl+C 2 回でクリップボード送信";
            _hostTextBox.PlaceholderText = "ホスト (例: api.openai.com)";
            _endpointTextBox.PlaceholderText = "エンドポイント (例: /v1/chat/completions)";
            _apiKeyTextBox.PlaceholderText = "API キー";
            _modelTextBox.PlaceholderText = "モデル名";
            _personalityOverrideTextBox.PlaceholderText = "人格プロンプト (任意)";
            _systemPromptTextBox.PlaceholderText = "追加のシステムプロンプト";
            _saveButton.Text = "保存";
            _cancelButton.Text = "キャンセル";
        }
    }

    private async Task SaveAsync()
    {
        UpdatedSettings = new AgentTalkSettings
        {
            English = _englishCheckBox.Checked,
            ExpandedTextbox = _expandedTextboxCheckBox.Checked,
            EnablePersonality = _enablePersonalityCheckBox.Checked,
            AutoCopyToClipboard = _autoCopyCheckBox.Checked,
            UseConversationHistory = _historyCheckBox.Checked,
            ClipboardHotkeyEnabled = _hotkeyCheckBox.Checked,
            ClipboardHotkeyIntervalMs = (int)_hotkeyIntervalNumeric.Value,
            Host = _hostTextBox.Text.Trim(),
            Endpoint = _endpointTextBox.Text.Trim(),
            ApiKey = _apiKeyTextBox.Text.Trim(),
            Model = _modelTextBox.Text.Trim(),
            PersonalityOverride = _personalityOverrideTextBox.Text,
            SystemPrompt = _systemPromptTextBox.Text,
            LastUserMessage = _workingCopy.LastUserMessage
        };

        if (string.IsNullOrEmpty(UpdatedSettings.Host))
        {
            MessageBox.Show(this, _englishCheckBox.Checked ? "Host is required." : "ホストは必須です。", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        if (string.IsNullOrEmpty(UpdatedSettings.Endpoint))
        {
            MessageBox.Show(this, _englishCheckBox.Checked ? "Endpoint is required." : "エンドポイントは必須です。", Text, MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }

        await _settingsService.SaveAsync(UpdatedSettings).ConfigureAwait(false);
        DialogResult = DialogResult.OK;
        Close();
    }

    private void UpdatePersonalityEditors()
    {
        _personalityOverrideTextBox.Enabled = _enablePersonalityCheckBox.Checked;
    }

    private static AgentTalkSettings CloneSettings(AgentTalkSettings source)
    {
        return new AgentTalkSettings
        {
            English = source.English,
            ExpandedTextbox = source.ExpandedTextbox,
            EnablePersonality = source.EnablePersonality,
            AutoCopyToClipboard = source.AutoCopyToClipboard,
            ClipboardHotkeyEnabled = source.ClipboardHotkeyEnabled,
            ClipboardHotkeyIntervalMs = source.ClipboardHotkeyIntervalMs,
            UseConversationHistory = source.UseConversationHistory,
            Host = source.Host,
            Endpoint = source.Endpoint,
            ApiKey = source.ApiKey,
            Model = source.Model,
            SystemPrompt = source.SystemPrompt,
            PersonalityOverride = source.PersonalityOverride,
            LastUserMessage = source.LastUserMessage
        };
    }
}
