using System;
using System.Linq;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OtakAgent.Core.Configuration;

namespace OtakAgent.Core.Chat
{
    /// <summary>
    /// Service for the modern OpenAI API /v1/responses endpoint
    /// </summary>
    public class ModernChatService
    {
        private readonly HttpClient _httpClient;
        private readonly JsonSerializerOptions _jsonOptions = new(JsonSerializerDefaults.Web)
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = false
        };

        public ModernChatService(HttpClient httpClient)
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

            // Check if we should use the modern endpoint based on model
            if (ShouldUseModernEndpoint(request.Settings.Model))
            {
                return await SendModernAsync(request, cancellationToken).ConfigureAwait(false);
            }

            // Fall back to legacy endpoint for older models
            return await SendLegacyAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private async Task<string> SendModernAsync(ChatRequest request, CancellationToken cancellationToken)
        {
            var modernRequest = BuildModernRequest(request);
            var uri = BuildModernUri(request.Settings);

            using var httpRequest = new HttpRequestMessage(HttpMethod.Post, uri)
            {
                Content = new StringContent(JsonSerializer.Serialize(modernRequest, _jsonOptions), Encoding.UTF8, "application/json")
            };

            httpRequest.Headers.Authorization = new AuthenticationHeaderValue("Bearer", request.Settings.ApiKey);

            using var timeoutCts = new CancellationTokenSource(TimeSpan.FromSeconds(60)); // Longer timeout for reasoning models
            using var linkedCts = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken, timeoutCts.Token);

            try
            {
                using var response = await _httpClient.SendAsync(httpRequest, HttpCompletionOption.ResponseHeadersRead, linkedCts.Token).ConfigureAwait(false);
                response.EnsureSuccessStatusCode();

                await using var stream = await response.Content.ReadAsStreamAsync(linkedCts.Token).ConfigureAwait(false);
                var modernResponse = await JsonSerializer.DeserializeAsync<ModernChatResponse>(stream, _jsonOptions, linkedCts.Token).ConfigureAwait(false)
                                ?? throw new InvalidOperationException("Failed to deserialize modern chat response.");

                return modernResponse.Output?.Text ?? string.Empty;
            }
            catch (TaskCanceledException) when (timeoutCts.IsCancellationRequested)
            {
                throw new TimeoutException($"Chat API request timed out after 60 seconds.");
            }
        }

        private async Task<string> SendLegacyAsync(ChatRequest request, CancellationToken cancellationToken)
        {
            // Use the existing ChatService logic for legacy endpoint
            var chatService = new ChatService(_httpClient);
            return await chatService.SendAsync(request, cancellationToken).ConfigureAwait(false);
        }

        private ModernChatRequest BuildModernRequest(ChatRequest request)
        {
            var modernRequest = new ModernChatRequest
            {
                Model = request.Settings.Model,
                Temperature = request.Temperature,
                TopP = request.TopP,
                MaxOutputTokens = request.MaxTokens,
                Text = new TextFormat
                {
                    Format = new Format { Type = "text" }
                }
            };

            // Add system message if present
            if (!string.IsNullOrWhiteSpace(request.SystemPrompt))
            {
                modernRequest.Input.Add(new InputMessage
                {
                    Role = "system",
                    Content = request.SystemPrompt
                });
            }

            // Add conversation history
            if (request.History != null && request.Settings.UseConversationHistory)
            {
                foreach (var message in request.History)
                {
                    modernRequest.Input.Add(new InputMessage
                    {
                        Role = message.Role,
                        Content = message.Content
                    });
                }
            }

            // Add user message
            modernRequest.Input.Add(new InputMessage
            {
                Role = "user",
                Content = request.UserMessage
            });

            // Enable reasoning for o1 models
            if (IsReasoningModel(request.Settings.Model))
            {
                modernRequest.Reasoning = new ReasoningConfig
                {
                    Effort = "medium"
                };
            }

            // Enable web search if requested
            if (request.Settings.EnableWebSearch)
            {
                modernRequest.Tools = new()
                {
                    new Tool
                    {
                        Type = "web_search",
                        UserLocation = new UserLocation { Type = "approximate" },
                        SearchContextSize = "low"
                    }
                };
                modernRequest.Include = new() { "web_search_call.action.sources" };
            }

            return modernRequest;
        }

        private bool ShouldUseModernEndpoint(string model)
        {
            // Use modern endpoint for newer models
            return model.Contains("gpt-4o", StringComparison.OrdinalIgnoreCase) ||
                   model.Contains("o1", StringComparison.OrdinalIgnoreCase) ||
                   model.Contains("gpt-4.1", StringComparison.OrdinalIgnoreCase);
        }

        private bool IsReasoningModel(string model)
        {
            return model.Contains("o1", StringComparison.OrdinalIgnoreCase);
        }

        private Uri BuildModernUri(AgentTalkSettings settings)
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
                var uri = new Uri(host);
                if (!uri.IsLoopback)
                {
                    host = "https" + host.Substring(4);
                }
            }

            host = host.TrimEnd('/');

            // Use modern endpoint
            var endpoint = "/v1/responses";

            return new Uri(host + endpoint, UriKind.Absolute);
        }
    }
}