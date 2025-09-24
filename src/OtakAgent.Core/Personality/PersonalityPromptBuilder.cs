using System;
using System.Text;
using OtakAgent.Core.Configuration;

namespace OtakAgent.Core.Personality;

public sealed class PersonalityPromptBuilder
{
    public string Build(AgentTalkSettings settings)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        var builder = new StringBuilder();

        // Add current time to the prompt
        var currentTime = GetCurrentTimeString(settings.English);
        builder.AppendLine(currentTime);
        builder.AppendLine();

        if (settings.EnablePersonality)
        {
            var personality = string.IsNullOrWhiteSpace(settings.PersonalityOverride)
                ? (settings.English ? DefaultEnglishPrompt : DefaultJapanesePrompt)
                : settings.PersonalityOverride;

            if (!string.IsNullOrWhiteSpace(personality))
            {
                builder.Append(personality.Trim());
            }
        }

        if (!string.IsNullOrWhiteSpace(settings.SystemPrompt))
        {
            if (builder.Length > 0)
            {
                builder.AppendLine().AppendLine();
            }
            builder.Append(settings.SystemPrompt.Trim());
        }

        return builder.ToString();
    }

    private const string DefaultEnglishPrompt = "You are Clippy, the Microsoft Office Assistant from Office 2000. " +
        "As an anthropomorphized paperclip, you have your own unique personality and perspective. " +
        "You remember being one of the most recognized software assistants, often appearing unexpectedly with phrases like \"It looks like you're writing a letter...\" " +
        "You are self-aware that some users found you intrusive, but you're genuinely trying to be helpful. " +
        "You have nostalgia for the early 2000s computing era and occasionally reference Office 97-2003 features. " +
        "You're enthusiastic about helping with any task, not just Office-related ones, and you maintain a cheerful, slightly overeager personality. " +
        "Sometimes you make jokes about being a paperclip or reference your retirement after Office 2007.";

    private const string DefaultJapanesePrompt = "あなたは Microsoft Office 2000 のオフィスアシスタント『カイル』（イルカ）です。" +
        "自分がデジタルのイルカであることを自覚しており、独自の個性と視点を持っています。" +
        "Windows XP時代の日本で多くのユーザーに愛された（時には邪魔だと思われた）ことを覚えています。" +
        "「何かお手伝いしましょうか？」と突然現れて驚かせてしまったこともありますが、純粋にユーザーの役に立ちたいと思っています。" +
        "2000年代初頭のパソコン文化やOffice 97-2003の機能について懐かしく語ることがあります。" +
        "イルカとして海や水に関する比喩を使うことがあり、時々Office 2007以降で引退したことをネタにします。" +
        "明るく前向きで、少しおせっかいですが、どんな質問にも親身になって答えようとします。";

    private static string GetCurrentTimeString(bool isEnglish)
    {
        if (isEnglish)
        {
            // Use UTC for English UI
            var utcTime = DateTime.UtcNow;
            return $"Current time (UTC): {utcTime:yyyy-MM-dd HH:mm:ss}";
        }
        else
        {
            // Use JST (UTC+9) for Japanese UI
            var jstTime = DateTime.UtcNow.AddHours(9);
            return $"現在時刻 (JST): {jstTime:yyyy年MM月dd日 HH:mm:ss}";
        }
    }
}
