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
            _saveButton.Text = "Save";
            _cancelButton.Text = "Cancel";
            _resetDefaultsButton.Text = "Reset to Defaults";
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
            _webSearchCheckBox.Text = "Web 検索を有効化 (対応モデルのみ)";
            _hostTextBox.PlaceholderText = "ホスト (例: api.openai.com)";
            _endpointTextBox.PlaceholderText = "エンドポイント (例: /v1/chat/completions)";
            _apiKeyTextBox.PlaceholderText = "API キー";
            _modelTextBox.PlaceholderText = "モデル名";
            _personalityOverrideTextBox.PlaceholderText = "人格プロンプト (任意)";
            _systemPromptTextBox.PlaceholderText = "追加のシステムプロンプト";
            _saveButton.Text = "保存";
            _cancelButton.Text = "キャンセル";
            _resetDefaultsButton.Text = "デフォルトに戻す";
        }
    }

    private async Task SaveAsync()
    {
        // Sanitize inputs before saving
        UpdatedSettings = new AgentTalkSettings
        {
            English = _englishCheckBox.Checked,
            ExpandedTextbox = _expandedTextboxCheckBox.Checked,
            EnablePersonality = _enablePersonalityCheckBox.Checked,
            AutoCopyToClipboard = _autoCopyCheckBox.Checked,
            UseConversationHistory = _historyCheckBox.Checked,
            ClipboardHotkeyEnabled = _hotkeyCheckBox.Checked,
            EnableWebSearch = _webSearchCheckBox.Checked,
            ClipboardHotkeyIntervalMs = (int)_hotkeyIntervalNumeric.Value,
            Host = _hostTextBox.Text.Trim(),
            Endpoint = _endpointTextBox.Text.Trim(),
            ApiKey = _apiKeyTextBox.Text.Trim(),
            Model = _modelTextBox.Text.Trim(),
            PersonalityOverride = SecureStringHelper.SanitizeInput(_personalityOverrideTextBox.Text),
            SystemPrompt = SecureStringHelper.SanitizeInput(_systemPromptTextBox.Text),
            LastUserMessage = _workingCopy.LastUserMessage
        };

        // Validate settings before saving
        var validation = SettingsValidator.ValidateSettings(UpdatedSettings);
        if (!validation.IsValid)
        {
            var message = validation.GetErrorMessage();
            var title = _englishCheckBox.Checked ? "Validation Error" : "検証エラー";
            MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Warning);

            // Only prevent saving if there are critical errors
            if (validation.Errors.Any(e => e.Contains("required")))
            {
                return;
            }
        }

        try
        {
            await _settingsService.SaveAsync(UpdatedSettings).ConfigureAwait(false);
            DialogResult = DialogResult.OK;
            Close();
        }
        catch (Exception ex)
        {
            var title = _englishCheckBox.Checked ? "Save Error" : "保存エラー";
            MessageBox.Show(this, ex.Message, title, MessageBoxButtons.OK, MessageBoxIcon.Error);
        }
    }

    private void UpdatePersonalityEditors()
    {
        _personalityOverrideTextBox.Enabled = _enablePersonalityCheckBox.Checked;
    }

    private void ResetToDefaults()
    {
        var defaults = AgentTalkSettings.CreateDefault();

        // Keep API key from current settings
        var currentApiKey = _apiKeyTextBox.Text;

        // Update all controls with default values
        _englishCheckBox.Checked = defaults.English;
        _expandedTextboxCheckBox.Checked = defaults.ExpandedTextbox;
        _enablePersonalityCheckBox.Checked = defaults.EnablePersonality;
        _autoCopyCheckBox.Checked = defaults.AutoCopyToClipboard;
        _historyCheckBox.Checked = defaults.UseConversationHistory;
        _hotkeyCheckBox.Checked = defaults.ClipboardHotkeyEnabled;
        _webSearchCheckBox.Checked = defaults.EnableWebSearch;
        _hotkeyIntervalNumeric.Value = defaults.ClipboardHotkeyIntervalMs;
        _hostTextBox.Text = defaults.Host;
        _endpointTextBox.Text = defaults.Endpoint;
        _apiKeyTextBox.Text = currentApiKey; // Keep the API key
        _modelTextBox.Text = defaults.Model;
        _personalityOverrideTextBox.Text = defaults.PersonalityOverride;
        _systemPromptTextBox.Text = defaults.SystemPrompt;

        UpdatePersonalityEditors();

        // Show confirmation message
        var message = _englishCheckBox.Checked
            ? "Settings have been reset to defaults (API key preserved)."
            : "設定をデフォルトに戻しました（APIキーは保持）。";
        var title = _englishCheckBox.Checked ? "Reset Complete" : "リセット完了";
        MessageBox.Show(this, message, title, MessageBoxButtons.OK, MessageBoxIcon.Information);
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
