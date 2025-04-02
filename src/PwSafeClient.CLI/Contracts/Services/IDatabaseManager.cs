using System.Collections.Generic;
using System.Threading.Tasks;

using PwSafeClient.Cli.Models;

namespace PwSafeClient.Cli.Contracts.Services
{
    internal interface IDatabaseManager
    {
        Task<List<Database>> ListDatabasesAsync();

        Task<string?> GetDbPathByAliasAsync(string alias);

        Task AddDatabaseAsync(string alias, string databasePath, bool isDefault = false, bool forceUpdate = false);

        Task RemoveDatabaseAsync(string alias);

        Task SetDefaultDatabaseAsync(string alias);
    }
}
