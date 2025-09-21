using AgentTalk.App.Composition;
using AgentTalk.App.Forms;
using AgentTalk.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace AgentTalk.App;

internal static class Program
{
    [STAThread]
    private static void Main()
    {
        ApplicationConfiguration.Initialize();
        using var serviceProvider = ServiceConfigurator.BuildServiceProvider();

        var bootstrapper = serviceProvider.GetRequiredService<SettingsBootstrapper>();
        var legacyLocation = serviceProvider.GetRequiredService<LegacySettingsLocation>();
        bootstrapper.EnsureInitializedAsync(legacyLocation.IniPath, legacyLocation.SystemPromptPath)
            .GetAwaiter().GetResult();

        var mainForm = serviceProvider.GetRequiredService<MainForm>();
        Application.Run(mainForm);
    }
}
