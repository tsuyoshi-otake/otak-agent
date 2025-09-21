using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace OtakAgent.Core.Configuration;

public sealed class SettingsBootstrapper
{
    private readonly SettingsService _settingsService;
    private readonly IniSettingsImporter _iniImporter;

    public SettingsBootstrapper(SettingsService settingsService, IniSettingsImporter iniImporter)
    {
        _settingsService = settingsService ?? throw new ArgumentNullException(nameof(settingsService));
        _iniImporter = iniImporter ?? throw new ArgumentNullException(nameof(iniImporter));
    }

    public async Task<AgentTalkSettings> EnsureInitializedAsync(string? legacyIniPath = null, string? systemPromptPath = null, CancellationToken cancellationToken = default)
    {
        if (!File.Exists(_settingsService.SettingsFilePath))
        {
            if (!string.IsNullOrWhiteSpace(legacyIniPath) && File.Exists(legacyIniPath))
            {
                var imported = _iniImporter.Import(legacyIniPath, systemPromptPath);
                await _settingsService.SaveAsync(imported, cancellationToken).ConfigureAwait(false);
                return imported;
            }
        }

        return await _settingsService.LoadAsync(cancellationToken).ConfigureAwait(false);
    }
}
