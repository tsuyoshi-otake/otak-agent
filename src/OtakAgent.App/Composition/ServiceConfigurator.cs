using System.IO;
using System.Net;
using System.Net.Http;
using OtakAgent.Core.Chat;
using OtakAgent.Core.Configuration;
using OtakAgent.Core.Http;
using OtakAgent.Core.Personality;
using OtakAgent.Core.Services;
using Microsoft.Extensions.DependencyInjection;
using Polly;
using Polly.Extensions.Http;

namespace OtakAgent.App.Composition;

internal static class ServiceConfigurator
{
    public static ServiceProvider BuildServiceProvider()
    {
        var services = new ServiceCollection();

        var baseDirectory = AppContext.BaseDirectory;
        var settingsPath = Path.Combine(baseDirectory, "agenttalk.settings.json");
        var legacyIniPath = Path.Combine(baseDirectory, "agenttalk.ini");
        var legacySystemPromptPath = Path.Combine(baseDirectory, "SystemPrompt.ini");

        services.AddSingleton(new SettingsService(settingsPath));
        services.AddSingleton(new IniSettingsImporter());
        services.AddSingleton<SettingsBootstrapper>();
        services.AddSingleton(new LegacySettingsLocation(legacyIniPath, legacySystemPromptPath));

        // Configure HttpClient with proxy support and retry policies
        services.AddHttpClient<ChatService>("ChatAPI", client =>
        {
            client.Timeout = TimeSpan.FromSeconds(30);
            client.DefaultRequestHeaders.Add("User-Agent", "OtakAgent/1.0");
        })
        .ConfigurePrimaryHttpMessageHandler(() => CreateHttpMessageHandler())
        .AddPolicyHandler(GetRetryPolicy())
        .AddPolicyHandler(GetCircuitBreakerPolicy());


        services.AddSingleton<PersonalityPromptBuilder>();
        services.AddSingleton<ISystemResourceService, SystemResourceService>();

        services.AddSingleton<Forms.MainForm>();

        return services.BuildServiceProvider();
    }

    private static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .OrResult(msg => msg.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
            .WaitAndRetryAsync(
                3,
                retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)),
                onRetry: (outcome, timespan, retryCount, context) =>
                {
                    var reason = outcome.Exception?.Message ?? $"Status: {outcome.Result?.StatusCode}";
                    System.Diagnostics.Debug.WriteLine($"Retry {retryCount} after {timespan}s. Reason: {reason}");
                });
    }

    private static IAsyncPolicy<HttpResponseMessage> GetCircuitBreakerPolicy()
    {
        return HttpPolicyExtensions
            .HandleTransientHttpError()
            .CircuitBreakerAsync(
                5,
                TimeSpan.FromMinutes(1),
                onBreak: (result, duration) =>
                {
                    System.Diagnostics.Debug.WriteLine($"Circuit breaker opened for {duration}");
                },
                onReset: () =>
                {
                    System.Diagnostics.Debug.WriteLine("Circuit breaker reset");
                });
    }

    private static HttpMessageHandler CreateHttpMessageHandler()
    {
        var handler = new HttpClientHandler
        {
            AutomaticDecompression = DecompressionMethods.GZip | DecompressionMethods.Deflate
        };

        // Only configure proxy if system proxy is detected
        try
        {
            var proxy = WebRequest.GetSystemWebProxy();
            if (proxy != null)
            {
                handler.UseProxy = true;
                handler.Proxy = proxy;
                handler.DefaultProxyCredentials = CredentialCache.DefaultCredentials;
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Proxy configuration failed: {ex.Message}");
            // Continue without proxy
        }

        return handler;
    }
}

internal sealed record LegacySettingsLocation(string IniPath, string SystemPromptPath);
