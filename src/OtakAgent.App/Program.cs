using OtakAgent.App.Composition;
using OtakAgent.App.Forms;
using OtakAgent.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OtakAgent.App;

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
