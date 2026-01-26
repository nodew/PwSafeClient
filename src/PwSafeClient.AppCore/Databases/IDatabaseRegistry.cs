using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace PwSafeClient.AppCore.Databases;

public interface IDatabaseRegistry
{
    Task<IReadOnlyList<DatabaseRegistration>> ListAsync(string? searchText = null, CancellationToken cancellationToken = default);

    Task<string?> TryGetPathAsync(string alias, CancellationToken cancellationToken = default);

    Task AddOrUpdateAsync(string alias, string filePath, bool makeDefault, CancellationToken cancellationToken = default);

    Task RemoveAsync(string alias, CancellationToken cancellationToken = default);

    Task SetDefaultAsync(string alias, CancellationToken cancellationToken = default);
}
