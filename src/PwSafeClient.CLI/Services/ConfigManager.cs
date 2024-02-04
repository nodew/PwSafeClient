using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Exceptions;
using PwSafeClient.CLI.Models;
using PwSafeClient.Shared;

namespace PwSafeClient.CLI.Services;

/// <summary>
/// The implementation of <see cref="IConfigManager"/>.
/// </summary>
public class ConfigManager : IConfigManager
{
    private static readonly JsonSerializerOptions options = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        WriteIndented = true
    };

    private const string configFolder = ".pwsafe";

    private const string configFilename = "config.json";

    private readonly string configFileAbsolutePath;

    private readonly string configFolderAbsolutePath;

    private readonly IConsoleService consoleService;

    public ConfigManager(IEnvironmentManager environmentManager, IConsoleService consoleService)
    {
        this.consoleService = consoleService;

        var homeDirectory = environmentManager.GetHomeDirectory();
        if (string.IsNullOrEmpty(homeDirectory))
        {
            throw new Exception("Cannot find home directory.");
        }

        configFolderAbsolutePath = Path.Combine(homeDirectory, configFolder);
        configFileAbsolutePath = Path.Combine(configFolderAbsolutePath, configFilename);
    }

    /// <inheritdoc/>
    public bool ConfigExists()
    {
        return File.Exists(configFileAbsolutePath);
    }

    /// <inheritdoc/>
    public string GetConfigPath()
    {
        return configFileAbsolutePath;
    }

    /// <inheritdoc/>
    public async Task AddDatabaseAsync(string alias, string filepath, bool isDefault = false)
    {
        ArgumentValidator.ThrowIfNullOrWhiteSpace(nameof(alias), alias);
        ArgumentValidator.ThrowIfNullOrWhiteSpace(nameof(filepath), filepath);

        var config = await LoadConfigAsync();
        config.Databases[alias] = filepath;

        if (isDefault)
        {
            config.DefaultDatabase = alias;
        }

        await SaveAsync(config);
    }

    /// <inheritdoc/>
    public async Task<string> GetDbPathAsync(string? alias)
    {
        var config = await LoadConfigAsync();
        alias ??= config.DefaultDatabase;

        if (string.IsNullOrEmpty(alias))
        {
            throw new Exception("The default database is not configured.");
        }

        if (config.Databases.TryGetValue(alias, out var filePath))
        {
            if (string.IsNullOrEmpty(filePath))
            {
                throw new DatabaseNotFoundException(alias);
            }
            else
            {
                return filePath;
            }
        }
        else
        {
            throw new DatabaseNotFoundException(alias);
        }
    }

    /// <inheritdoc/>
    public async Task<int> GetIdleTimeAsync()
    {
        var config = await LoadConfigAsync();
        return config.IdleTime;
    }

    /// <inheritdoc/>
    public async Task<int> GetMaxBackupCountAsync()
    {
        var config = await LoadConfigAsync();
        return config.MaxBackupCount;
    }

    /// <inheritdoc/>
    public async Task<Config> LoadConfigAsync()
    {
        try
        {
            using var fileStream = File.Open(configFileAbsolutePath, FileMode.Open);
            using var reader = new StreamReader(fileStream);
            var json = await reader.ReadToEndAsync();

            var config = JsonSerializer.Deserialize<Config>(json, options);
            return config ?? new Config();
        }
        catch (FileNotFoundException)
        {
            consoleService.LogError($"Cannot find config file: {configFileAbsolutePath}");
            throw;
        }
        catch (DirectoryNotFoundException)
        {
            consoleService.LogError($"Cannot find config file: {configFileAbsolutePath}");
            throw;
        }
        catch (JsonException)
        {
            consoleService.LogError($"Cannot parse config file: {configFileAbsolutePath}, please double check the config.");
            throw;
        }
        catch (Exception e)
        {
            consoleService.LogError(e.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RemoveDatabaseAsync(string alias)
    {
        ArgumentValidator.ThrowIfNullOrWhiteSpace(nameof(alias), alias);

        var config = await LoadConfigAsync();
        config.Databases.Remove(alias);

        if (config.DefaultDatabase == alias)
        {
            config.DefaultDatabase = null;
        }

        await SaveAsync(config);
    }

    /// <inheritdoc/>
    public Task SaveAsync(Config config)
    {
        try
        {
            if (!Directory.Exists(configFolderAbsolutePath))
            {
                Directory.CreateDirectory(configFolderAbsolutePath);
            }

            var content = JsonSerializer.Serialize(config, options);
            return File.WriteAllTextAsync(configFileAbsolutePath, content);
        }
        catch (Exception e)
        {
            consoleService.LogError(e.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SetDefaultDatabaseAsync(string alias)
    {
        ArgumentValidator.ThrowIfNullOrWhiteSpace(nameof(alias), alias);

        var config = await LoadConfigAsync();

        foreach (var item in config.Databases)
        {
            if (item.Key == alias)
            {
                config.DefaultDatabase = alias;
                await SaveAsync(config);
                return;
            }
        }

        throw new DatabaseNotFoundException(alias);
    }
}
