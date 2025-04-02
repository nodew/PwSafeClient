using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

using PwSafeClient.Cli.Contracts.Services;
using PwSafeClient.Cli.Models;

namespace PwSafeClient.Cli.Services
{
    internal class DatabaseManager : IDatabaseManager
    {
        private readonly IConfigManager _configManager;

        public DatabaseManager(IConfigManager configManager)
        {
            _configManager = configManager;
        }

        public async Task AddDatabaseAsync(string alias, string databasePath, bool isDefault = false, bool forceUpdate = false)
        {
            var configuration = await _configManager.LoadConfigurationAsync();

            if (!forceUpdate && configuration.Databases.ContainsKey(alias))
            {
                throw new InvalidOperationException($"Database with alias '{alias}' already exists.");
            }

            configuration.Databases[alias] = databasePath;

            if (isDefault)
            {
                configuration.DefaultDatabase = alias;
            }

            await _configManager.SaveConfigurationAsync(configuration);
        }

        public async Task<List<Database>> ListDatabasesAsync()
        {
            var configuration = await _configManager.LoadConfigurationAsync();
            var databases = configuration.Databases
                .Select(x => new Database(x.Key, x.Value, IsDefault: x.Key.Equals(configuration.DefaultDatabase, StringComparison.OrdinalIgnoreCase)))
                .ToList();
            return databases;
        }

        public async Task<string?> GetDbPathByAliasAsync(string alias)
        {
            var configuration = await _configManager.LoadConfigurationAsync();
            return configuration.Databases.GetValueOrDefault(alias);
        }

        public async Task RemoveDatabaseAsync(string alias)
        {
            var configuration = await _configManager.LoadConfigurationAsync();
            configuration.Databases.Remove(alias);
            await _configManager.SaveConfigurationAsync(configuration);
        }

        public async Task SetDefaultDatabaseAsync(string alias)
        {
            var configuration = await _configManager.LoadConfigurationAsync();

            if (!configuration.Databases.ContainsKey(alias))
            {
                throw new InvalidOperationException($"Database with alias '{alias}' does not exist.");
            }

            configuration.DefaultDatabase = alias;
            await _configManager.SaveConfigurationAsync(configuration);
        }
    }
}
