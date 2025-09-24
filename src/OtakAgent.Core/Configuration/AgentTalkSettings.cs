using System.Collections.Generic;

namespace OtakAgent.Core.Configuration;

public sealed class AgentTalkSettings
{
    public bool English { get; set; } = true;
    public bool ExpandedTextbox { get; set; }
    public bool EnablePersonality { get; set; }
    public bool AutoCopyToClipboard { get; set; }
    public bool ClipboardHotkeyEnabled { get; set; } = true;
    public int ClipboardHotkeyIntervalMs { get; set; } = 500;
    public bool UseConversationHistory { get; set; } = true;
    public string Host { get; set; } = "api.openai.com";
    public string Endpoint { get; set; } = "/v1/responses";
    public string ApiKey { get; set; } = string.Empty;
    public string Model { get; set; } = "gpt-4.1";
    public string SystemPrompt { get; set; } = string.Empty;
    public string PersonalityOverride { get; set; } = string.Empty;
    public string LastUserMessage { get; set; } = string.Empty;
    public bool EnableWebSearch { get; set; } = false;
    public List<SystemPromptPreset> SystemPromptPresets { get; set; } = new();
    public string SelectedPresetId { get; set; } = string.Empty;

    public static AgentTalkSettings CreateDefault() => new();
}
