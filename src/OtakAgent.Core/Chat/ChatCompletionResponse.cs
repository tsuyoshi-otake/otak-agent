using System.Text.Json.Serialization;

namespace OtakAgent.Core.Chat;

public sealed class ChatCompletionResponse
{
    [JsonPropertyName("choices")]
    public IList<ChatCompletionChoice> Choices { get; set; } = new List<ChatCompletionChoice>();
}

public sealed class ChatCompletionChoice
{
    [JsonPropertyName("message")]
    public ChatCompletionMessage? Message { get; set; }
}

public sealed class ChatCompletionMessage
{
    [JsonPropertyName("role")]
    public string Role { get; set; } = string.Empty;

    [JsonPropertyName("content")]
    public string Content { get; set; } = string.Empty;
}
