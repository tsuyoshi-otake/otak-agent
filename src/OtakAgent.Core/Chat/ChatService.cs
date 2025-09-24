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

        // Determine which API to use based on model
        var useResponsesApi = IsResponsesApiModel(request.Settings.Model);

        // Set timeout for HTTP request
        using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(30));
        using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

        try
        {
            if (useResponsesApi)
            {
                return await SendResponsesApiAsync(request, linkedCts.Token).ConfigureAwait(false);
            }
            else
            {
                return await SendChatCompletionsApiAsync(request, linkedCts.Token).ConfigureAwait(false);
            }
        }
        catch (TaskCanceledException) when (timeoutCts.IsCancellationRequested)
        {
            throw new TimeoutException($"Chat API request timed out after 30 seconds.");
        }
    }

    private async Task<string> SendChatCompletionsApiAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        var payload = BuildPayload(request);
        var uri = BuildTargetUri(request.Settings);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Settings.ApiKey);

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
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

    private async Task<string> SendResponsesApiAsync(ChatRequest request, CancellationToken cancellationToken)
    {
        var payload = BuildResponsesApiPayload(request);
        var uri = BuildResponsesApiUri(request.Settings);

        using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
        {
            Content = new StringContent(JsonSerializer.Serialize(payload, _jsonOptions), Encoding.UTF8, "application/json")
        };

        httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Settings.ApiKey);

        using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, cancellationToken).ConfigureAwait(false);
        response.EnsureSuccessStatusCode();

        var responseText = await response.Content.ReadAsStringAsync(cancellationToken).ConfigureAwait(false);
        var apiResponse = JsonSerializer.Deserialize<ResponsesApiResponse>(responseText, _jsonOptions)
                        ?? throw new InvalidOperationException("Failed to deserialize responses API response.");

        // Extract text from the response - try different possible structures
        if (apiResponse.Output != null && apiResponse.Output.Count > 0)
        {
            // First try to find a message type output
            var messageOutput = apiResponse.Output.FirstOrDefault(o => o.Type == "message");
            if (messageOutput?.Content != null && messageOutput.Content.Count > 0)
            {
                // Check for both "text" and "output_text" types
                var textContent = messageOutput.Content.FirstOrDefault(c => c.Type == "text" || c.Type == "output_text");
                if (!string.IsNullOrEmpty(textContent?.Text))
                {
                    return textContent.Text;
                }
            }

            // If no message type, check all outputs for text content
            foreach (var output in apiResponse.Output)
            {
                if (output.Content != null && output.Content.Count > 0)
                {
                    var textContent = output.Content.FirstOrDefault(c => c.Type == "text" || c.Type == "output_text");
                    if (!string.IsNullOrEmpty(textContent?.Text))
                    {
                        return textContent.Text;
                    }
                }
            }

            // Debug: Log the actual response structure and JSON
            var debugInfo = string.Join("; ", apiResponse.Output.Select(o =>
                $"Type={o.Type}, ContentCount={o.Content?.Count ?? 0}, ContentTypes={string.Join(",", o.Content?.Select(c => c.Type) ?? Array.Empty<string>())}"));

            // Include the raw JSON for debugging
            throw new InvalidOperationException($"Response did not include any text content. Debug: {debugInfo}\n\nRaw JSON:\n{responseText}");
        }

        throw new InvalidOperationException($"Response did not include any output. Output count: {apiResponse.Output?.Count ?? 0}");
    }

    private static bool IsResponsesApiModel(string model)
    {
        var lowerModel = model?.ToLowerInvariant() ?? string.Empty;
        return lowerModel.Contains("gpt-4.1") ||
               lowerModel.Contains("gpt-4o") ||
               lowerModel.StartsWith("o1") ||
               lowerModel.StartsWith("o3");
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

    private static ResponsesApiRequest BuildResponsesApiPayload(ChatRequest request)
    {
        var systemPrompt = request.SystemPrompt ?? string.Empty;
        var conversationHistory = string.Empty;

        if (request.History != null && request.Settings.UseConversationHistory)
        {
            var historyMessages = request.History
                .Select(h => $"{h.Role}: {h.Content}")
                .ToList();
            if (historyMessages.Any())
            {
                conversationHistory = "\n\nPrevious conversation:\n" + string.Join("\n", historyMessages);
            }
        }

        var fullInput = string.IsNullOrWhiteSpace(systemPrompt)
            ? request.UserMessage + conversationHistory
            : $"{systemPrompt}\n\nUser: {request.UserMessage}{conversationHistory}";

        var payload = new ResponsesApiRequest
        {
            Model = request.Settings.Model,
            Input = fullInput,
            MaxOutputTokens = request.MaxTokens,
            Temperature = request.Temperature,
            TopP = request.TopP
        };

        // Add web search tool if enabled
        if (request.Settings.EnableWebSearch)
        {
            payload.Tools = new List<object>
            {
                new Dictionary<string, object>
                {
                    ["type"] = "web_search",
                    ["user_location"] = new Dictionary<string, string>
                    {
                        ["type"] = "approximate"
                    },
                    ["search_context_size"] = "low"
                }
            };
        }

        return payload;
    }

    private static Uri BuildResponsesApiUri(AgentTalkSettings settings)
    {
        var host = settings.Host?.Trim() ?? string.Empty;
        if (host.Length == 0)
        {
            host = "https://api.openai.com";
        }

        // Ensure HTTPS unless localhost
        if (!host.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            host = $"https://{host}";
        }
        else if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            // Check if it's localhost/127.0.0.1
            var uri = new Uri(host);
            if (!uri.IsLoopback)
            {
                // Force HTTPS for non-localhost connections
                host = "https" + host.Substring(4);
            }
        }

        host = host.TrimEnd('/');

        // For responses API, always use /v1/responses
        return new Uri($"{host}/v1/responses", UriKind.Absolute);
    }

    private static Uri BuildTargetUri(AgentTalkSettings settings)
    {
        var host = settings.Host?.Trim() ?? string.Empty;
        if (host.Length == 0)
        {
            host = "https://api.openai.com";
        }

        // Ensure HTTPS unless localhost
        if (!host.StartsWith("http", StringComparison.OrdinalIgnoreCase))
        {
            host = $"https://{host}";
        }
        else if (host.StartsWith("http://", StringComparison.OrdinalIgnoreCase))
        {
            // Check if it's localhost/127.0.0.1
            var uri = new Uri(host);
            if (!uri.IsLoopback)
            {
                // Force HTTPS for non-localhost connections
                host = "https" + host.Substring(4);
            }
        }

        host = host.TrimEnd('/');

        var endpoint = settings.Endpoint?.Trim() ?? string.Empty;
        if (!endpoint.StartsWith('/'))
        {
            endpoint = $"/{endpoint}";
        }

        // Sanitize endpoint to prevent directory traversal
        endpoint = endpoint.Replace("..", "").Replace("\\", "/");

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
