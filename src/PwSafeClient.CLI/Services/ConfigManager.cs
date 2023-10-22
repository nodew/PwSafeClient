using PwSafeClient.CLI.Contracts.Services;
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

    public ConfigManager(IEnvironmentManager environmentManager)
    {
        string? homeDirectory = environmentManager.GetHomeDirectory();
        if (string.IsNullOrEmpty(homeDirectory))
        {
            throw new Exception("Cannot find home directory.");
        }

        configFilepath = Path.Combine(homeDirectory, configFilename);
    }

    /// <inheritdoc/>
    public async Task AddDatabase(string alias, string filepath, bool isDefault = false)
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
    public async Task<string> GetDbPath(string? alias)
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
                throw new Exception($"The database with alias: {alias} is not configured.");
            }
            else
            {
                return filePath;
            }
        }
        else
        {
            throw new Exception($"Cannot find database with alias: {alias}");
        }
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
            Console.Error.WriteLine($"Cannot find config file: {configFilepath}");
            throw;
        }
        catch (DirectoryNotFoundException)
        {
            Console.Error.WriteLine($"Cannot find config file: {configFilepath}");
            throw;
        }
        catch (JsonException)
        {
            Console.Error.WriteLine($"Cannot parse config file: {configFilepath}, please double check the config.");
            throw;
        }
        catch (Exception e)
        {
            Console.Error.WriteLine(e.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task RemoveDatabase(string alias)
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
            Console.Error.WriteLine(e.Message);
            throw;
        }
    }

    /// <inheritdoc/>
    public async Task SetDefaultDatabase(string alias)
    {
        ArgumentValidator.ThrowIfNullOrWhiteSpace(nameof(alias), alias);

        Config config = await LoadConfigAsync();
        config.DefaultDatabase = alias;
        await SaveAsync(config);
    }
}
