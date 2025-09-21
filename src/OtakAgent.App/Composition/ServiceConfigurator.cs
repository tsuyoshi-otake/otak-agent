using System.IO;
using OtakAgent.Core.Chat;
using OtakAgent.Core.Configuration;
using OtakAgent.Core.Personality;
using Microsoft.Extensions.DependencyInjection;

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

        services.AddHttpClient<ChatService>();
        services.AddSingleton<PersonalityPromptBuilder>();

        services.AddSingleton<Forms.MainForm>();

        return services.BuildServiceProvider();
    }
}

internal sealed record LegacySettingsLocation(string IniPath, string SystemPromptPath);
