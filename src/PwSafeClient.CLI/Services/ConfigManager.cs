using PwSafeClient.CLI.Contracts.Services;
using PwSafeClient.CLI.Exceptions;
using PwSafeClient.CLI.Models;
using PwSafeClient.Shared;
using System;
using System.IO;
using System.Text.Json;
using System.Threading.Tasks;

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

    private const string configFilename = "pwsafe.json";

    private readonly string configFilepath;

    private readonly IConsoleService consoleService;

    public ConfigManager(IEnvironmentManager environmentManager, IConsoleService consoleService)
    {
        this.consoleService = consoleService;

        string? homeDirectory = environmentManager.GetHomeDirectory();
        if (string.IsNullOrEmpty(homeDirectory))
        {
            throw new Exception("Cannot find home directory.");
        }

        configFilepath = Path.Combine(homeDirectory, configFilename);
    }

    public bool ConfigExists()
    {
        return File.Exists(configFilepath);
    }

    /// <inheritdoc/>
    public async Task AddDatabaseAsync(string alias, string filepath, bool isDefault = false)
    {
        ArgumentValidator.ThrowIfNullOrWhiteSpace(nameof(alias), alias);
        ArgumentValidator.ThrowIfNullOrWhiteSpace(nameof(filepath), filepath);

        Config config = await LoadConfigAsync();
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
        Config config = await LoadConfigAsync();
        alias ??= config.DefaultDatabase;

        if (string.IsNullOrEmpty(alias))
        {
            throw new Exception("The default database is not configured.");
        }

        if (config.Databases.TryGetValue(alias, out string? filePath))
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
        Config config = await LoadConfigAsync();
        return config.IdleTime;
    }

    /// <inheritdoc/>
    public async Task<int> GetMaxBackupCountAsync()
    {
        Config config = await LoadConfigAsync();
        return config.MaxBackupCount;
    }

    /// <inheritdoc/>
    public async Task<Config> LoadConfigAsync()
    {
        try
        {
            using FileStream fileStream = File.Open(configFilepath, FileMode.Open);
            using StreamReader reader = new StreamReader(fileStream);
            string json = await reader.ReadToEndAsync();

            Config? config = JsonSerializer.Deserialize<Config>(json, options);
            return config ?? new Config();
        }
        catch (FileNotFoundException)
        {
            consoleService.LogError($"Cannot find config file: {configFilepath}");
            throw;
        }
        catch (DirectoryNotFoundException)
        {
            consoleService.LogError($"Cannot find config file: {configFilepath}");
            throw;
        }
        catch (JsonException)
        {
            consoleService.LogError($"Cannot parse config file: {configFilepath}, please double check the config.");
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

        Config config = await LoadConfigAsync();
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
            string content = JsonSerializer.Serialize(config, options);
            return File.WriteAllTextAsync(configFilepath, content);
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

        Config config = await LoadConfigAsync();

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
