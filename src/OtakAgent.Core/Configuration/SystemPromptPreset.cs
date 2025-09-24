namespace OtakAgent.Core.Configuration;

public sealed class SystemPromptPreset
{
    public string Id { get; set; } = Guid.NewGuid().ToString();
    public string Name { get; set; } = string.Empty;
    public string Prompt { get; set; } = string.Empty;
    public bool IsBuiltIn { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    public static List<SystemPromptPreset> GetBuiltInPresets()
    {
        return new List<SystemPromptPreset>
        {
            new()
            {
                Name = "Default Assistant",
                Prompt = "You are a helpful assistant.",
                IsBuiltIn = true
            },
            new()
            {
                Name = "Code Review",
                Prompt = "You are a code reviewer. Analyze the provided code for bugs, performance issues, and best practices. Provide constructive feedback.",
                IsBuiltIn = true
            },
            new()
            {
                Name = "Japanese Translator",
                Prompt = "あなたは日本語の翻訳者です。提供されたテキストを自然な日本語に翻訳してください。",
                IsBuiltIn = true
            },
            new()
            {
                Name = "Proofreader",
                Prompt = "校閲してください。校閲後の文章を出力してください。",
                IsBuiltIn = true
            },
            new()
            {
                Name = "Technical Writer",
                Prompt = "You are a technical writer. Write clear, concise documentation with proper formatting and examples.",
                IsBuiltIn = true
            },
            new()
            {
                Name = "Creative Writer",
                Prompt = "You are a creative writer. Write engaging and imaginative content with vivid descriptions.",
                IsBuiltIn = true
            }
        };
    }
}