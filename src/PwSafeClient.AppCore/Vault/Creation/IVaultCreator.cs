using System.Threading;
using System.Threading.Tasks;

namespace PwSafeClient.AppCore.Vault.Creation;

public interface IVaultCreator
{
    Task<(string Alias, string FilePath)> CreateNewAsync(
        string databaseName,
        string masterPassword,
        string? destinationFilePath = null,
        CancellationToken cancellationToken = default);
}
