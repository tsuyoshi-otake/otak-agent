using System.Text.Json.Serialization;

namespace OtakAgent.Core.Chat;

public sealed class ChatCompletionRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("messages")]
    public IList<ChatMessagePayload> Messages { get; set; } = new List<ChatMessagePayload>();

    [JsonPropertyName("max_tokens")]
    public int MaxTokens { get; set; } = 32768;

    [JsonPropertyName("temperature")]
    public double Temperature { get; set; } = 1.0;

    [JsonPropertyName("top_p")]
    public double TopP { get; set; } = 1.0;

    [JsonPropertyName("frequency_penalty")]
    public double FrequencyPenalty { get; set; } = 0.0;

    [JsonPropertyName("presence_penalty")]
    public double PresencePenalty { get; set; } = 0.0;
}

public sealed class ChatMessagePayload
{
    public ChatMessagePayload(string role, string content)
    {
        Role = role;
        Content = content;
    }

    [JsonPropertyName("role")]
    public string Role { get; }

    [JsonPropertyName("content")]
    public string Content { get; }
}
