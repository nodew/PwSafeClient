using System.Threading.Tasks;

namespace PwSafeClient.AppCore.Security.Secrets;

public interface ISecureSecretStore
{
    Task SaveAsync(string key, string secret);

    Task<string?> TryGetAsync(string key);

    Task RemoveAsync(string key);
}
