using System.IO;
using System.Linq;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;
using OtakAgent.Core.Security;
using OtakAgent.Core.Validation;

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

        _settingsFilePath = GetAppropriateSettingsPath(settingsFilePath);
        _jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
        {
            WriteIndented = true,
            PropertyNameCaseInsensitive = true
        };
    }

    private static string GetAppropriateSettingsPath(string defaultPath)
    {
        var baseDirectory = AppContext.BaseDirectory;

        // Check if we're installed in Program Files (requires admin to write)
        var programFiles = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFiles);
        var programFilesX86 = Environment.GetFolderPath(Environment.SpecialFolder.ProgramFilesX86);

        if (baseDirectory.StartsWith(programFiles, StringComparison.OrdinalIgnoreCase) ||
            baseDirectory.StartsWith(programFilesX86, StringComparison.OrdinalIgnoreCase))
        {
            // Use AppData for settings when installed in Program Files
            var appDataPath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "OtakAgent"
            );

            // Ensure directory exists
            Directory.CreateDirectory(appDataPath);

            return Path.Combine(appDataPath, "agenttalk.settings.json");
        }

        // For portable version, use the provided path (next to exe)
        return defaultPath;
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
            var settings = await JsonSerializer.DeserializeAsync<AgentTalkSettings>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false)
                           ?? AgentTalkSettings.CreateDefault();

            // Decrypt API key if it was encrypted
            if (!string.IsNullOrEmpty(settings.ApiKey))
            {
                settings.ApiKey = SecureStringHelper.Unprotect(settings.ApiKey);
            }

            // Sanitize inputs
            settings.SystemPrompt = SecureStringHelper.SanitizeInput(settings.SystemPrompt);
            settings.PersonalityOverride = SecureStringHelper.SanitizeInput(settings.PersonalityOverride);
            settings.LastUserMessage = SecureStringHelper.SanitizeInput(settings.LastUserMessage);

            // Ensure HTTPS and sanitize URLs
            settings.Host = SettingsValidator.EnsureHttps(settings.Host);
            settings.Endpoint = SettingsValidator.SanitizeEndpoint(settings.Endpoint);

            return settings;
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
        // Validate settings before saving
        var validation = SettingsValidator.ValidateSettings(settings);
        if (!validation.IsValid && validation.Errors.Any(e => e.Contains("required")))
        {
            throw new InvalidOperationException($"Invalid settings: {validation.GetErrorMessage()}");
        }

        // Create a copy for saving with encrypted API key
        var settingsToSave = new AgentTalkSettings
        {
            English = settings.English,
            ExpandedTextbox = settings.ExpandedTextbox,
            EnablePersonality = settings.EnablePersonality,
            AutoCopyToClipboard = settings.AutoCopyToClipboard,
            UseConversationHistory = settings.UseConversationHistory,
            Host = SettingsValidator.EnsureHttps(settings.Host),
            Endpoint = SettingsValidator.SanitizeEndpoint(settings.Endpoint),
            ApiKey = SecureStringHelper.Protect(settings.ApiKey),  // Encrypt the API key
            Model = settings.Model,
            SystemPrompt = SecureStringHelper.SanitizeInput(settings.SystemPrompt),
            PersonalityOverride = SecureStringHelper.SanitizeInput(settings.PersonalityOverride),
            LastUserMessage = SecureStringHelper.SanitizeInput(settings.LastUserMessage)
        };

        Directory.CreateDirectory(Path.GetDirectoryName(_settingsFilePath)!);
        await using var stream = File.Create(_settingsFilePath);
        await JsonSerializer.SerializeAsync(stream, settingsToSave, _jsonOptions, cancellationToken).ConfigureAwait(false);
    }

    private async Task<AgentTalkSettings> LoadUnlockedAsync(CancellationToken cancellationToken)
    {
        if (!File.Exists(_settingsFilePath))
        {
            return AgentTalkSettings.CreateDefault();
        }

        await using var stream = File.OpenRead(_settingsFilePath);
        var settings = await JsonSerializer.DeserializeAsync<AgentTalkSettings>(stream, _jsonOptions, cancellationToken).ConfigureAwait(false)
                       ?? AgentTalkSettings.CreateDefault();

        // Decrypt API key if it was encrypted
        if (!string.IsNullOrEmpty(settings.ApiKey))
        {
            settings.ApiKey = SecureStringHelper.Unprotect(settings.ApiKey);
        }

        // Sanitize inputs
        settings.SystemPrompt = SecureStringHelper.SanitizeInput(settings.SystemPrompt);
        settings.PersonalityOverride = SecureStringHelper.SanitizeInput(settings.PersonalityOverride);
        settings.LastUserMessage = SecureStringHelper.SanitizeInput(settings.LastUserMessage);

        // Ensure HTTPS and sanitize URLs
        settings.Host = SettingsValidator.EnsureHttps(settings.Host);
        settings.Endpoint = SettingsValidator.SanitizeEndpoint(settings.Endpoint);

        return settings;
    }
}
