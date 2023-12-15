using PwSafeClient.CLI.Models;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Contracts.Services;

/// <summary>
/// Manage the configuration of the CLI.
/// </summary>
public interface IConfigManager
{
    /// <summary>
    /// Check if the configuration file exists.
    /// </summary>
    /// <returns>If the configuration file exists.</returns>
    bool ConfigExists();

    /// <summary>
    /// Add a new database to the configuration.
    /// </summary>
    /// <param name="alias">The alias of the database.</param>
    /// <param name="filepath">The absolute filepath.</param>
    /// <param name="isDefault">Set the database as default.</param>
    /// <returns></returns>
    Task AddDatabaseAsync(string alias, string filepath, bool isDefault = false);

    /// <summary>
    /// Remove a database from the configuration.
    /// </summary>
    /// <param name="alias">The alias of target database.</param>
    /// <returns></returns>
    Task RemoveDatabaseAsync(string alias);

    /// <summary>
    /// Set the default database.
    /// </summary>
    /// <param name="alias">The alias of target database.</param>
    /// <returns></returns>
    Task SetDefaultDatabaseAsync(string alias);

    /// <summary>
    /// Get the absolute path of the database.
    /// </summary>
    /// <param name="alias">The alias of the database.</param>
    /// <returns></returns>
    Task<string> GetDbPathAsync(string? alias);

    /// <summary>
    /// Get the idle time in minutes before the interactive terminal exits.
    /// </summary>
    /// <returns></returns>
    Task<int> GetIdleTimeAsync();

    /// <summary>
    /// Get the maximum number of backup files to keep.
    /// </summary>
    /// <returns></returns>
    Task<int> GetMaxBackupCountAsync();

    /// <summary>
    /// Save the configuration.
    /// </summary>
    /// <param name="config">The configuration.</param>
    /// <returns></returns>
    Task SaveAsync(Config config);

    /// <summary>
    /// Load the configuration.
    /// </summary>
    /// <returns>The configuration.</returns>
    Task<Config> LoadConfigAsync();
}
