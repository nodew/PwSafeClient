using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Exceptions;
using PwSafeClient.Cli.Models;

namespace PwSafeClient.Cli.Services;

internal class ConfigManager : IConfigManager
{
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private const string configFolder = ".pwsafe";
    private const string configFilename = "config.json";

    private readonly IEnvironmentManager _environmentManager;
    private readonly string configFileAbsolutePath;
    private readonly string configFolderAbsolutePath;

    public ConfigManager(IEnvironmentManager environmentManager)
    {
        _environmentManager = environmentManager;
        var homeDirectory = _environmentManager.GetHomeDirectory();

        if (string.IsNullOrEmpty(homeDirectory))
        {
            throw new Exception("Cannot find home directory.");
        }

        configFolderAbsolutePath = Path.Combine(homeDirectory, configFolder);
        configFileAbsolutePath = Path.Combine(configFolderAbsolutePath, configFilename);
    }

    public bool IsConfigurationExists()
    {
        return File.Exists(configFileAbsolutePath);
    }

    public async Task InitConfigurationAsync()
    {
        if (IsConfigurationExists())
        {
            throw new ConfigurationAlreadyExistsException($"\"{configFileAbsolutePath}\" already exists.");
        }

        if (!Directory.Exists(configFolderAbsolutePath))
        {
            Directory.CreateDirectory(configFolderAbsolutePath);
        }

        var config = new Configuration();
        await SaveConfigurationAsync(config);
    }

    public async Task<Configuration> LoadConfigurationAsync()
    {
        if (!IsConfigurationExists())
        {
            throw new FailedToLoadConfigurationException($"\"{configFileAbsolutePath}\" doesn't exist.");
        }

        try
        {
            using var fileStream = File.Open(configFileAbsolutePath, FileMode.Open);
            using var reader = new StreamReader(fileStream);
            var json = await reader.ReadToEndAsync();

            var config = JsonSerializer.Deserialize<Configuration>(json, options);
            return config ?? new Configuration();
        }
        catch (JsonException)
        {
            throw new FailedToLoadConfigurationException($"\"{configFileAbsolutePath}\" is not a valid configuration.");
        }
        catch (Exception e)
        {
            throw new FailedToLoadConfigurationException($"Failed to load configuration: {e.Message}");
        }
    }

    public Task SaveConfigurationAsync(Configuration configuration)
    {
        if (!IsConfigurationExists())
        {
            throw new FailedToSaveConfigurationException($"\"{configFileAbsolutePath}\" doesn't exist.");
        }

        try
        {
            var json = JsonSerializer.Serialize(configuration, options);
            File.WriteAllText(configFileAbsolutePath, json);
            return Task.CompletedTask;
        }
        catch (Exception e)
        {
            throw new FailedToSaveConfigurationException($"Failed to save configuration: {e.Message}");
        }
    }
}
