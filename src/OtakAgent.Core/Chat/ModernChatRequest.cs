using System.Collections.Generic;
using System.Text.Json.Serialization;

namespace OtakAgent.Core.Chat
{
    /// <summary>
    /// Modern OpenAI API request for /v1/responses endpoint
    /// </summary>
    public class ModernChatRequest
    {
        [JsonPropertyName("model")]
        public string Model { get; set; } = "gpt-4o-mini";

        [JsonPropertyName("input")]
        public List<InputMessage> Input { get; set; } = new();

        [JsonPropertyName("text")]
        public TextFormat? Text { get; set; }

        [JsonPropertyName("reasoning")]
        public ReasoningConfig? Reasoning { get; set; }

        [JsonPropertyName("tools")]
        public List<Tool>? Tools { get; set; }

        [JsonPropertyName("temperature")]
        public double Temperature { get; set; } = 0.7;

        [JsonPropertyName("max_output_tokens")]
        public int MaxOutputTokens { get; set; } = 32768;

        [JsonPropertyName("top_p")]
        public double TopP { get; set; } = 1.0;

        [JsonPropertyName("store")]
        public bool Store { get; set; } = false;

        [JsonPropertyName("include")]
        public List<string>? Include { get; set; }
    }

    public class InputMessage
    {
        [JsonPropertyName("role")]
        public string Role { get; set; } = "user";

        [JsonPropertyName("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class TextFormat
    {
        [JsonPropertyName("format")]
        public Format Format { get; set; } = new();
    }

    public class Format
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "text";
    }

    public class ReasoningConfig
    {
        [JsonPropertyName("effort")]
        public string? Effort { get; set; }
    }

    public class Tool
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = string.Empty;

        [JsonPropertyName("user_location")]
        public UserLocation? UserLocation { get; set; }

        [JsonPropertyName("search_context_size")]
        public string? SearchContextSize { get; set; }
    }

    public class UserLocation
    {
        [JsonPropertyName("type")]
        public string Type { get; set; } = "approximate";
    }
}