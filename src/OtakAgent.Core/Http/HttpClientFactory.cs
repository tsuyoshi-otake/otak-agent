using System;
using System.Net;
using System.Net.Http;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace OtakAgent.Core.Http
{
    public static class HttpClientFactory
    {
        /// <summary>
        /// Creates an HttpClient with retry policy and proxy support
        /// </summary>
        public static HttpClient CreateWithRetryPolicy()
        {
            var handler = new HttpClientHandler
            {
                // Auto-detect system proxy settings
                UseProxy = true,
                Proxy = WebRequest.GetSystemWebProxy(),
                DefaultProxyCredentials = CredentialCache.DefaultCredentials,

                // Allow automatic decompression
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate,

                // Certificate validation (can be customized if needed)
                ServerCertificateCustomValidationCallback = (sender, cert, chain, sslPolicyErrors) => true
            };

            var client = new HttpClient(handler)
            {
                Timeout = TimeSpan.FromSeconds(30)
            };

            // Set default headers
            client.DefaultRequestHeaders.Add("User-Agent", "OtakAgent/1.0");
            client.DefaultRequestHeaders.Add("Accept", "application/json");

            return client;
        }

        /// <summary>
        /// Creates retry policy for transient HTTP errors
        /// </summary>
        public static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError() // Handles HttpRequestException and 5XX, 408 status codes
                .OrResult(msg => msg.StatusCode == HttpStatusCode.TooManyRequests)
                .WaitAndRetryAsync(
                    3, // Retry 3 times
                    retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), // Exponential backoff
                    onRetry: (outcome, timespan, retryCount, context) =>
                    {
                        var reason = outcome.Exception?.Message ?? $"Status: {outcome.Result?.StatusCode}";
                        System.Diagnostics.Debug.WriteLine($"Retry {retryCount} after {timespan}s. Reason: {reason}");
                    });
        }

        /// <summary>
        /// Configure HttpClient services with Polly
        /// </summary>
        public static void ConfigureHttpClient(IServiceCollection services)
        {
            services.AddHttpClient<Chat.ChatService>("ChatAPI", client =>
            {
                client.Timeout = TimeSpan.FromSeconds(30);
                client.DefaultRequestHeaders.Add("User-Agent", "OtakAgent/1.0");
            })
            .ConfigurePrimaryHttpMessageHandler(() => new HttpClientHandler
            {
                UseProxy = true,
                Proxy = WebRequest.GetSystemWebProxy(),
                DefaultProxyCredentials = CredentialCache.DefaultCredentials,
                AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
            })
            .AddPolicyHandler(GetRetryPolicy())
            .AddPolicyHandler(GetCircuitBreakerPolicy());
        }

        /// <summary>
        /// Circuit breaker to prevent overwhelming a failing service
        /// </summary>
        private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .CircuitBreakerAsync(
                    5, // Break after 5 consecutive failures
                    TimeSpan.FromMinutes(1), // Stay open for 1 minute
                    onBreak: (result, duration) =>
                    {
                        System.Diagnostics.Debug.WriteLine($"Circuit breaker opened for {duration}");
                    },
                    onReset: () =>
                    {
                        System.Diagnostics.Debug.WriteLine("Circuit breaker reset");
                    });
        }
    }
}