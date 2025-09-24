namespace OtakAgent.Core.Configuration;

public sealed class SystemPromptPreset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static List<SystemPromptPreset> GetBuiltInPresets(bool isEnglish = true)
    {
        if (isEnglish)
        {
            return new List<SystemPromptPreset>
            {
                new()
                {
                    Id = "default-assistant",
                    Name = "Default Assistant",
                    Prompt = "You are a helpful assistant.",
                    IsBuiltIn = true
                },
                new()
                {
                    Id = "code-review",
                    Name = "Code Review",
                    Prompt = "You are a code reviewer. Analyze the provided code for bugs, performance issues, and best practices. Provide constructive feedback.",
                    IsBuiltIn = true
                },
                new()
                {
                    Id = "japanese-translator",
                    Name = "Japanese Translator",
                    Prompt = "Translate into Japanese and return only the translation. Do not include any prefixes or explanations.",
                    IsBuiltIn = true
                },
                new()
                {
                    Id = "proofreader",
                    Name = "Proofreader",
                    Prompt = "Return only the corrected text. Do not include any prefixes like 'Corrected text:' or any explanations. Output only the proofread text itself.",
                    IsBuiltIn = true
                },
                new()
                {
                    Id = "technical-writer",
                    Name = "Technical Writer",
                    Prompt = "You are a technical writer. Write clear, concise documentation with proper formatting and examples.",
                    IsBuiltIn = true
                },
                new()
                {
                    Id = "creative-writer",
                    Name = "Creative Writer",
                    Prompt = "You are a creative writer. Write engaging and imaginative content with vivid descriptions.",
                    IsBuiltIn = true
                }
            };
        }
        else
        {
            return new List<SystemPromptPreset>
            {
                new()
                {
                    Id = "default-assistant",
                    Name = "デフォルトアシスタント",
                    Prompt = "あなたは親切なアシスタントです。マークダウンではなくテキストで簡潔に回答してください。",
                    IsBuiltIn = true
                },
                new()
                {
                    Id = "code-review",
                    Name = "コードレビュー",
                    Prompt = "あなたはコードレビュアーです。提供されたコードのバグ、パフォーマンス問題、ベストプラクティスを分析し、建設的なフィードバックを提供してください。",
                    IsBuiltIn = true
                },
                new()
                {
                    Id = "japanese-translator",
                    Name = "英日翻訳",
                    Prompt = "日本語に翻訳した文章だけを返してください。「翻訳：」などの前置きや説明は一切不要です。",
                    IsBuiltIn = true
                },
                new()
                {
                    Id = "proofreader",
                    Name = "校閲者",
                    Prompt = "文法、誤字、句読点、文体の問題を修正した文章だけを返してください。「校閲後の文章：」などの前置きや説明は一切不要です。修正した文章そのものだけを出力してください。",
                    IsBuiltIn = true
                },
                new()
                {
                    Id = "technical-writer",
                    Name = "技術文書作成",
                    Prompt = "あなたは技術文書の作成者です。適切な書式と例を用いて、明確で簡潔なドキュメントを作成してください。",
                    IsBuiltIn = true
                },
                new()
                {
                    Id = "creative-writer",
                    Name = "創作ライター",
                    Prompt = "あなたは創作ライターです。鮮明な描写で魅力的で想像力豊かなコンテンツを書いてください。",
                    IsBuiltIn = true
                }
            };
        }
    }
}