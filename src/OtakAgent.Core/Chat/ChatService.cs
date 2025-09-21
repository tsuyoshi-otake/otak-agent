using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using OtakAgent.Core.Configuration;

namespace OtakAgent.Core.Chat;

public sealed class ChatService
{
    private readonly HttpClient _httpClient;
    private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = false
    };

    public ChatService(HttpClient httpClient)
    {
        _httpClient = httpClient ?? throw new ArgumentNullException(nameof(httpClient));
    }

    public async Task<string> SendAsync(ChatRequest request, CancellationToken cancellationToken = default)
    {
        if (request == null)
        {
            throw new ArgumentNullException(nameof(request));
        }

        if (string.IsNullOrWhiteSpace(request.Settings.ApiKey))
        {
            throw new InvalidOperationException("API key is required to send chat requests.");
        }

        var payload = BuildPayload(request);
        var uri = BuildTargetUri(request.Settings);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Settings.ApiKey);

        using var response = await _httpClient.SendAsync(httpRequest, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        await using var stream = await response.Content.ReadAsStreamAsync(cancellationToken).ConfigureAwait(false);
        var completion = await JsonSerializer.DeserializeAsync<ChatCompletionResponse>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false)
                        ?? throw new InvalidOperationException("Failed to deserialize chat completion response.");

        var message = completion.Choices.FirstOrDefault()?.Message;
        if (message == null)
        {
            throw new InvalidOperationException("Chat completion response did not include any message choices.");
        }

        return message.Content;
    }

    private static ChatCompletionRequest BuildPayload(ChatRequest request)
    {
        var payload = new ChatCompletionRequest
        {
            Model = request.Settings.Model,
            MaxTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP,
            FrequencyPenalty = request.FrequencyPenalty,
            PresencePenalty = request.PresencePenalty
        };

        if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
        {
            payload.Messages.Add(new ChatMessagePayload("system", request.SystemPrompt));
        }

        if (request.History != null && request.Settings.UseConversationHistory)
        {
            foreach (var historyMessage in request.History)
            {
                payload.Messages.Add(new ChatMessagePayload(historyMessage.Role, historyMessage.Content));
            }
        }

        payload.Messages.Add(new ChatMessagePayload("user", request.UserMessage));
        return payload;
    }

    private static Uri BuildTargetUri(AgentTalkSettings settings)
    {
        var host = settings.Host?.Trim() ?? string.Empty;
        if (host.Length == 0)
        {
            host = "https://api.openai.com";
        }

        if (!host.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            host = $"https://{host}";
        }

        host = host.TrimEnd('/');

        var endpoint = settings.Endpoint?.Trim() ?? string.Empty;
        if (!endpoint.StartsWith('/'))
        {
            endpoint = $"/{endpoint}";
        }

        return new Uri(host + endpoint, UriKind.Absolute);
    }
}

public sealed record ChatRequest(
    AgentTalkSettings Settings,
    string UserMessage,
    IReadOnlyList<ChatMessage>? History = null,
    string? SystemPrompt = null,
    int MaxTokens = 1024,
    double Temperature = 1.0,
    double TopP = 1.0,
    double FrequencyPenalty = 0.0,
    double PresencePenalty = 0.0);
