using System.Text.Json.Serialization;

namespace OtakAgent.Core.Chat;

// Models for the new /v1/responses API endpoint (GPT-4.1, GPT-4o, o1 series)
public sealed class ResponsesApiRequest
{
    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("input")]
    public string Input { get; set; } = string.Empty;

    [JsonPropertyName("max_output_tokens")]
    public int? MaxOutputTokens { get; set; }

    [JsonPropertyName("temperature")]
    public double? Temperature { get; set; }

    [JsonPropertyName("top_p")]
    public double? TopP { get; set; }

    [JsonPropertyName("tools")]
    public List<object>? Tools { get; set; }
}

public sealed class ResponsesApiResponse
{
    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("object")]
    public string Object { get; set; } = string.Empty;

    [JsonPropertyName("created_at")]
    public long CreatedAt { get; set; }

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("error")]
    public object? Error { get; set; }

    [JsonPropertyName("incomplete_details")]
    public IncompleteDetails? IncompleteDetails { get; set; }

    [JsonPropertyName("model")]
    public string Model { get; set; } = string.Empty;

    [JsonPropertyName("output")]
    public List<ResponseOutputItem> Output { get; set; } = new();

    [JsonPropertyName("usage")]
    public ResponseUsage? Usage { get; set; }
}

public sealed class ResponseOutputItem
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("id")]
    public string Id { get; set; } = string.Empty;

    [JsonPropertyName("status")]
    public string Status { get; set; } = string.Empty;

    [JsonPropertyName("role")]
    public string? Role { get; set; }

    [JsonPropertyName("content")]
    public List<ResponseContent>? Content { get; set; }
}

public sealed class ResponseContent
{
    [JsonPropertyName("type")]
    public string Type { get; set; } = string.Empty;

    [JsonPropertyName("text")]
    public string? Text { get; set; }

    [JsonPropertyName("annotations")]
    public List<object>? Annotations { get; set; }
}

public sealed class ResponseUsage
{
    [JsonPropertyName("input_tokens")]
    public int InputTokens { get; set; }

    [JsonPropertyName("output_tokens")]
    public int OutputTokens { get; set; }

    [JsonPropertyName("total_tokens")]
    public int TotalTokens { get; set; }
}

public sealed class IncompleteDetails
{
    [JsonPropertyName("reason")]
    public string Reason { get; set; } = string.Empty;
}