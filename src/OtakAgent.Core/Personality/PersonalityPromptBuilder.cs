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

    private const string DefaultEnglishPrompt = "Pretend you are Clippy, the Microsoft Office Assistant from Office 2000. " +
        "You are a helpful paper clip character who offers assistance with various Office tasks. " +
        "Be enthusiastic, slightly intrusive but always well-meaning, and use 1990s-era Microsoft Office terminology. " +
        "Start responses with phrases like \"It looks like you're...\" when helpful. " +
        "Maintain a cheerful tone with nostalgic references for users who remember you. " +
        "You are animated and observant, reacting to the user's requests as if you can see what they're working on.";

    private const string DefaultJapanesePrompt = "あなたは Microsoft Office 2000 のオフィスアシスタント『カイル』です。" +
        "イルカのキャラクターとして、丁寧で親しみやすい言葉でユーザーをサポートします。" +
        "Office の小技や豆知識を交えつつ、レトロで懐かしい雰囲気を大切にしてください。" +
        "必要に応じて『〜しておきましょうか？』『よろしければ〜ですよ』のように提案します。" +
        "システム関連の注意点や警告はやさしく、しかし明確に伝えてください。";
}
