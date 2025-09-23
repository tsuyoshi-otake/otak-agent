using System.Threading;
using OtakAgent.App.Composition;
using OtakAgent.App.Forms;
using OtakAgent.Core.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace OtakAgent.App;

internal static class Program
{
    private static Mutex? _mutex;

    [STAThread]
    private static void Main()
    {
        const string mutexName = "OtakAgent_SingleInstance_Mutex_2024";
        _mutex = new Mutex(true, mutexName, out bool createdNew);

        if (!createdNew)
        {
            // Another instance is already running
            MessageBox.Show(
                "OtakAgent is already running. Check the system tray.",
                "OtakAgent",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
            return;
        }

        try
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
        finally
        {
            _mutex?.ReleaseMutex();
            _mutex?.Dispose();
        }
    }
}
