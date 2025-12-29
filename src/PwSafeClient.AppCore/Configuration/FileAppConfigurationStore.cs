using System;
using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace PwSafeClient.AppCore.Configuration;

public sealed class FileAppConfigurationStore : IAppConfigurationStore
{
    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true,
        Converters =
        {
            new JsonStringEnumConverter(JsonNamingPolicy.CamelCase)
        }
    };

    private readonly IAppPaths _paths;

    public FileAppConfigurationStore(IAppPaths paths)
    {
        _paths = paths;
    }

    public async Task<AppConfiguration> LoadAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var configPath = GetConfigFilePath();

        if (!File.Exists(configPath))
        {
            return new AppConfiguration();
        }

        var json = await File.ReadAllTextAsync(configPath, cancellationToken);

        var config = JsonSerializer.Deserialize<AppConfiguration>(json, SerializerOptions) ?? new AppConfiguration();

        // Back-compat: older configs stored a boolean `isDarkModeEnabled` only.
        // If `theme` is absent, migrate the legacy value.
        try
        {
            using var doc = JsonDocument.Parse(json);
            var root = doc.RootElement;

            if (!root.TryGetProperty("theme", out _)
                && root.TryGetProperty("isDarkModeEnabled", out var legacy)
                && (legacy.ValueKind == JsonValueKind.True || legacy.ValueKind == JsonValueKind.False))
            {
                config.Theme = legacy.GetBoolean() ? AppThemePreference.Dark : AppThemePreference.Light;
            }
        }
        catch
        {
            // ignore; best-effort migration
        }

        return config;
    }

    public async Task SaveAsync(AppConfiguration configuration, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var configPath = GetConfigFilePath();
        var folder = Path.GetDirectoryName(configPath);

        if (!string.IsNullOrWhiteSpace(folder) && !Directory.Exists(folder))
        {
            Directory.CreateDirectory(folder);
        }

        var json = JsonSerializer.Serialize(configuration, SerializerOptions);
        await File.WriteAllTextAsync(configPath, json, cancellationToken);
    }

    private string GetConfigFilePath()
    {
        var root = _paths.AppDataDirectory;
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("AppDataDirectory is not available.");
        }

        return Path.Combine(root, "pwsafe", "config.json");
    }
}
