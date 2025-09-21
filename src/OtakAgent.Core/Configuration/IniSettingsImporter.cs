using System.IO;
using System.Text;

namespace OtakAgent.Core.Configuration;

public sealed class IniSettingsImporter
{
    public AgentTalkSettings Import(string iniFilePath, string? systemPromptPath = null)
    {
        if (string.IsNullOrWhiteSpace(iniFilePath))
        {
            throw new ArgumentException("INI file path must be provided.", nameof(iniFilePath));
        }

        if (!File.Exists(iniFilePath))
        {
            throw new FileNotFoundException("INI file not found.", iniFilePath);
        }

        var settings = AgentTalkSettings.CreateDefault();
        foreach (var rawLine in File.ReadAllLines(iniFilePath, Encoding.UTF8))
        {
            var line = rawLine.Trim();
            if (line.Length == 0 || line.StartsWith(";") || line.StartsWith("#") || line.StartsWith("["))
            {
                continue;
            }

            var separatorIndex = line.IndexOf('=');
            if (separatorIndex <= 0)
            {
                continue;
            }

            var key = line[..separatorIndex].Trim().ToLowerInvariant();
            var value = line[(separatorIndex + 1)..].Trim();

            switch (key)
            {
                case "english":
                    settings.English = ParseBoolean(value, true);
                    break;
                case "expandedtextbox":
                    settings.ExpandedTextbox = ParseBoolean(value, false);
                    break;
                case "enablepersonality":
                    settings.EnablePersonality = ParseBoolean(value, false);
                    break;
                case "autocopytoclipboard":
                    settings.AutoCopyToClipboard = ParseBoolean(value, false);
                    break;
                case "host":
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        settings.Host = value;
                    }
                    break;
                case "endpoint":
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        settings.Endpoint = value;
                    }
                    break;
                case "apikey":
                    settings.ApiKey = value;
                    break;
                case "model":
                    if (!string.IsNullOrWhiteSpace(value))
                    {
                        settings.Model = value;
                    }
                    break;
                case "useconversationhistory":
                case "usehistory":
                    settings.UseConversationHistory = ParseBoolean(value, true);
                    break;
                case "clipboardhotkeyenabled":
                    settings.ClipboardHotkeyEnabled = ParseBoolean(value, true);
                    break;
                case "clipboardhotkeyintervalms":
                    if (int.TryParse(value, out var interval) && interval > 0)
                    {
                        settings.ClipboardHotkeyIntervalMs = interval;
                    }
                    break;
                case "personalityoverride":
                    settings.PersonalityOverride = value;
                    break;
                case "systemprompt":
                    settings.SystemPrompt = value;
                    break;
                case "lastusermessage":
                    settings.LastUserMessage = value;
                    break;
            }
        }

        if (!string.IsNullOrEmpty(systemPromptPath) && File.Exists(systemPromptPath))
        {
            settings.SystemPrompt = File.ReadAllText(systemPromptPath, Encoding.UTF8);
        }

        return settings;
    }

    private static bool ParseBoolean(string value, bool fallback)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            return fallback;
        }

        if (bool.TryParse(value, out var result))
        {
            return result;
        }

        return value.Trim() switch
        {
            "1" or "yes" or "y" or "on" => true,
            "0" or "no" or "n" or "off" => false,
            _ => fallback
        };
    }
}
