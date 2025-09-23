using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using OtakAgent.Core.Configuration;

namespace OtakAgent.Core.Validation
{
    public static class SettingsValidator
    {
        // Common OpenAI-compatible models
        private static readonly HashSet<string> ValidModels = new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            // OpenAI models
            "gpt-4", "gpt-4-turbo", "gpt-4-turbo-preview", "gpt-4o", "gpt-4o-mini",
            "gpt-3.5-turbo", "gpt-3.5-turbo-16k",
            // Claude models (for Anthropic API)
            "claude-3-opus", "claude-3-sonnet", "claude-3-haiku",
            "claude-2.1", "claude-2", "claude-instant",
            // Allow custom models with specific prefixes
            "custom-", "local-", "test-"
        };

        private static readonly Regex ApiKeyPattern = new Regex(@"^(sk-[a-zA-Z0-9]{48,}|[a-zA-Z0-9\-_]{20,})$", RegexOptions.Compiled);
        private static readonly Regex UrlPattern = new Regex(@"^https?://[a-zA-Z0-9\-._~:/?#[\]@!$&'()*+,;=]+$", RegexOptions.Compiled);

        public static ValidationResult ValidateSettings(AgentTalkSettings settings)
        {
            var errors = new List<string>();

            // Validate API Key
            if (!string.IsNullOrWhiteSpace(settings.ApiKey))
            {
                var apiKey = settings.ApiKey.Trim();
                if (apiKey.Length < 20)
                {
                    errors.Add("API key appears to be too short. Please check your API key.");
                }
                // Don't be too strict about API key format as different providers have different formats
            }
            else
            {
                errors.Add("API key is required for chat functionality.");
            }

            // Validate Host
            if (!string.IsNullOrWhiteSpace(settings.Host))
            {
                var host = settings.Host.Trim();

                // Add https:// if no protocol specified
                if (!host.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                    !host.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
                {
                    host = "https://" + host;
                }

                // Validate URL format
                if (!Uri.TryCreate(host, UriKind.Absolute, out var uri))
                {
                    errors.Add($"Invalid host URL format: {settings.Host}");
                }
                else if (!uri.Scheme.Equals("https", StringComparison.OrdinalIgnoreCase) &&
                         !uri.IsLoopback) // Allow http for localhost only
                {
                    errors.Add("HTTPS is required for API connections (except localhost).");
                }
            }

            // Validate Endpoint
            if (!string.IsNullOrWhiteSpace(settings.Endpoint))
            {
                var endpoint = settings.Endpoint.Trim();
                if (!endpoint.StartsWith("/"))
                {
                    errors.Add("Endpoint should start with '/'");
                }

                // Check for suspicious characters that might indicate injection
                if (endpoint.Contains("..") || endpoint.Contains("\\"))
                {
                    errors.Add("Endpoint contains invalid characters.");
                }
            }
            else
            {
                errors.Add("Endpoint is required.");
            }

            // Validate Model
            if (!string.IsNullOrWhiteSpace(settings.Model))
            {
                var model = settings.Model.Trim();
                bool isValid = ValidModels.Contains(model) ||
                               ValidModels.Any(validModel => model.StartsWith(validModel, StringComparison.OrdinalIgnoreCase));

                if (!isValid && !model.Contains("gpt") && !model.Contains("claude") && !model.Contains("llama"))
                {
                    errors.Add($"Unknown model: {model}. This might still work if your API supports it.");
                }
            }
            else
            {
                errors.Add("Model name is required.");
            }

            // Validate numeric ranges
            if (settings.ClipboardHotkeyIntervalMs < 100 || settings.ClipboardHotkeyIntervalMs > 5000)
            {
                errors.Add("Clipboard hotkey interval should be between 100ms and 5000ms.");
            }

            return new ValidationResult
            {
                IsValid = errors.Count == 0,
                Errors = errors
            };
        }

        /// <summary>
        /// Ensures the host URL uses HTTPS (except for localhost)
        /// </summary>
        public static string EnsureHttps(string url)
        {
            if (string.IsNullOrWhiteSpace(url))
                return url;

            url = url.Trim();

            // Add protocol if missing
            if (!url.StartsWith("http://", StringComparison.OrdinalIgnoreCase) &&
                !url.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                return "https://" + url;
            }

            // Check if it's localhost/loopback
            if (Uri.TryCreate(url, UriKind.Absolute, out var uri))
            {
                if (uri.IsLoopback)
                {
                    return url; // Allow http for localhost
                }

                // Force HTTPS for non-localhost
                if (uri.Scheme.Equals("http", StringComparison.OrdinalIgnoreCase))
                {
                    var builder = new UriBuilder(uri)
                    {
                        Scheme = "https",
                        Port = uri.Port == 80 ? 443 : uri.Port
                    };
                    return builder.ToString();
                }
            }

            return url;
        }

        /// <summary>
        /// Sanitizes and validates the endpoint path
        /// </summary>
        public static string SanitizeEndpoint(string endpoint)
        {
            if (string.IsNullOrWhiteSpace(endpoint))
                return "/v1/chat/completions"; // Default OpenAI endpoint

            endpoint = endpoint.Trim();

            // Ensure it starts with /
            if (!endpoint.StartsWith("/"))
                endpoint = "/" + endpoint;

            // Remove dangerous patterns
            endpoint = endpoint.Replace("..", "");
            endpoint = endpoint.Replace("\\", "/");

            // Remove duplicate slashes
            endpoint = Regex.Replace(endpoint, @"/+", "/");

            return endpoint;
        }
    }

    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public List<string> Errors { get; set; } = new List<string>();

        public string GetErrorMessage()
        {
            return string.Join("\n", Errors);
        }
    }
}