using System;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OtakAgent.Core.Updates;

public class UpdateChecker
{
    private readonly HttpClient _httpClient;
    private const string GitHubApiUrl = "https://api.github.com/repos/tsuyoshi-otake/otak-agent/releases/latest";
    private const string GitHubReleaseUrl = "https://github.com/tsuyoshi-otake/otak-agent/releases/latest";

    public UpdateChecker(HttpClient httpClient)
    {
        _httpClient = httpClient;
        _httpClient.DefaultRequestHeaders.UserAgent.ParseAdd("OtakAgent-UpdateChecker/1.0");
    }

    public async Task<UpdateInfo?> CheckForUpdatesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            var response = await _httpClient.GetAsync(GitHubApiUrl, cancellationToken);
            if (!response.IsSuccessStatusCode)
                return null;

            var json = await response.Content.ReadAsStringAsync(cancellationToken);
            var release = JsonSerializer.Deserialize<GitHubRelease>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });

            if (release == null || string.IsNullOrEmpty(release.TagName))
                return null;

            // Parse version from tag (v1.0.0 -> 1.0.0)
            var latestVersion = release.TagName.TrimStart('v');
            var currentVersion = GetCurrentVersion();

            if (IsNewerVersion(currentVersion, latestVersion))
            {
                return new UpdateInfo
                {
                    CurrentVersion = currentVersion,
                    LatestVersion = latestVersion,
                    ReleaseUrl = GitHubReleaseUrl,
                    ReleaseNotes = release.Body ?? "",
                    PublishedAt = release.PublishedAt
                };
            }

            return null;
        }
        catch
        {
            // Silently fail - updates are not critical
            return null;
        }
    }

    private string GetCurrentVersion()
    {
        // Try to get version from assembly
        var assembly = Assembly.GetEntryAssembly() ?? Assembly.GetExecutingAssembly();
        var version = assembly.GetName().Version;

        if (version != null && version != new Version(1, 0, 0, 0))
        {
            return $"{version.Major}.{version.Minor}.{version.Build}";
        }

        // Default version if not set
        return "1.0.0";
    }

    private bool IsNewerVersion(string current, string latest)
    {
        try
        {
            var currentParts = current.Split('.').Select(int.Parse).ToArray();
            var latestParts = latest.Split('.').Select(int.Parse).ToArray();

            for (int i = 0; i < Math.Min(currentParts.Length, latestParts.Length); i++)
            {
                if (latestParts[i] > currentParts[i])
                    return true;
                if (latestParts[i] < currentParts[i])
                    return false;
            }

            return latestParts.Length > currentParts.Length;
        }
        catch
        {
            return false;
        }
    }
}

public class UpdateInfo
{
    public string CurrentVersion { get; set; } = "";
    public string LatestVersion { get; set; } = "";
    public string ReleaseUrl { get; set; } = "";
    public string ReleaseNotes { get; set; } = "";
    public DateTime PublishedAt { get; set; }
}

public class GitHubRelease
{
    public string TagName { get; set; } = "";
    public string Name { get; set; } = "";
    public string Body { get; set; } = "";
    public DateTime PublishedAt { get; set; }
}