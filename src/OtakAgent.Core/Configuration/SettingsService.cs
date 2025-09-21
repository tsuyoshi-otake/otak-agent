using System.IO;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OtakAgent.Core.Configuration;

public sealed class SettingsService
{
    private readonly SemaphoreSlim _mutex = new(1, 1);
    private readonly JsonSerializerOptions _jsonOptions;
    private readonly string _settingsFilePath;

    public SettingsService(string settingsFilePath)
    {
        if (string.IsNullOrWhiteSpace(settingsFilePath))
        {
            throw new ArgumentException("Settings file path must be provided.", nameof(settingsFilePath));
        }

        _settingsFilePath = settingsFilePath;
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    public string SettingsFilePath => _settingsFilePath;

    public async Task<AgentTalkSettings> LoadAsync(CancellationToken cancellationToken = default)
    {
        await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            if (!File.Exists(_settingsFilePath))
            {
                var defaults = AgentTalkSettings.CreateDefault();
                await SaveInternalAsync(defaults, cancellationToken).ConfigureAwait(false);
                return defaults;
            }

            await using var stream = File.OpenRead(_settingsFilePath);
            var settings = await JsonSerializer.DeserializeAsync<AgentTalkSettings>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false);
            return settings ?? AgentTalkSettings.CreateDefault();
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task SaveAsync(AgentTalkSettings settings, CancellationToken cancellationToken = default)
    {
        if (settings == null)
        {
            throw new ArgumentNullException(nameof(settings));
        }

        await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            await SaveInternalAsync(settings, cancellationToken).ConfigureAwait(false);
        }
        finally
        {
            _mutex.Release();
        }
    }

    public async Task<AgentTalkSettings> UpdateAsync(Func<AgentTalkSettings, Task> updateAction, CancellationToken cancellationToken = default)
    {
        if (updateAction == null)
        {
            throw new ArgumentNullException(nameof(updateAction));
        }

        await _mutex.WaitAsync(cancellationToken).ConfigureAwait(false);
        try
        {
            var current = await LoadUnlockedAsync(cancellationToken).ConfigureAwait(false);
            await updateAction(current).ConfigureAwait(false);
            await SaveInternalAsync(current, cancellationToken).ConfigureAwait(false);
            return current;
        }
        finally
        {
            _mutex.Release();
        }
    }

    private async Task SaveInternalAsync(AgentTalkSettings settings, CancellationToken cancellationToken)
    {
        Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);
        await using var stream = File.Create(_settingsFilePath);
        await JsonSerializer.SerializeAsync(stream, settings, _jsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AgentTalkSettings> LoadUnlockedAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_settingsFilePath))
        {
            return AgentTalkSettings.CreateDefault();
        }

        await using var stream = File.OpenRead(_settingsFilePath);
        var settings = await JsonSerializer.DeserializeAsync<AgentTalkSettings>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false);
        return settings ?? AgentTalkSettings.CreateDefault();
    }
}
