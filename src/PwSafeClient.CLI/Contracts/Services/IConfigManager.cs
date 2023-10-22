using PwSafeClient.CLI.Models;
using System.Threading.Tasks;

namespace PwSafeClient.CLI.Contracts.Services;

/// <summary>
/// Manage the configuration of the CLI.
/// </summary>
public interface IConfigManager
{
    /// <summary>
    /// Add a new database to the configuration.
    /// </summary>
    /// <param name="alias">The alias of the database.</param>
    /// <param name="filepath">The absolute filepath.</param>
    /// <param name="isDefault">Set the database as default.</param>
    /// <returns></returns>
    Task AddDatabase(string alias, string filepath, bool isDefault = false);

    /// <summary>
    /// Remove a database from the configuration.
    /// </summary>
    /// <param name="alias">The alias of target database.</param>
    /// <returns></returns>
    Task RemoveDatabase(string alias);

    /// <summary>
    /// Set the default database.
    /// </summary>
    /// <param name="alias">The alias of target database.</param>
    /// <returns></returns>
    Task SetDefaultDatabase(string alias);

    /// <summary>
    /// Get the absolute path of the database.
    /// </summary>
    /// <param name="alias">The alias of the database.</param>
    /// <returns></returns>
    Task<string> GetDbPath(string? alias);

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
