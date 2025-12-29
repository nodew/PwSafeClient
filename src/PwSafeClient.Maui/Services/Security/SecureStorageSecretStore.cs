using System;
using System.Threading.Tasks;

using PwSafeClient.AppCore.Security.Secrets;

namespace PwSafeClient.Maui.Services.Security;

public sealed class SecureStorageSecretStore : ISecureSecretStore
{
    public Task SaveAsync(string key, string secret)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));
        if (secret is null) throw new ArgumentNullException(nameof(secret));

        return SecureStorage.Default.SetAsync(key, secret);
    }

    public async Task<string?> TryGetAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));

        try
        {
            return await SecureStorage.Default.GetAsync(key);
        }
        catch
        {
            return null;
        }
    }

    public Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));

        try
        {
            SecureStorage.Default.Remove(key);
        }
        catch
        {
            // ignore
        }

        return Task.CompletedTask;
    }
}
