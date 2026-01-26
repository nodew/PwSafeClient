#if WINDOWS
using System;
using System.Threading.Tasks;

using PwSafeClient.AppCore.Security.Secrets;

using Windows.Security.Credentials;

namespace PwSafeClient.Maui.Platforms.Windows.Services;

public sealed class WindowsCredentialLockerSecretStore : ISecureSecretStore
{
    private const string UserName = "PwSafeClient";

    public Task SaveAsync(string key, string secret)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));
        if (secret is null) throw new ArgumentNullException(nameof(secret));

        var vault = new PasswordVault();

        // Remove existing credential if present.
        try
        {
            var existing = vault.Retrieve(key, UserName);
            vault.Remove(existing);
        }
        catch
        {
            // ignore
        }

        vault.Add(new PasswordCredential(key, UserName, secret));
        return Task.CompletedTask;
    }

    public Task<string?> TryGetAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));

        try
        {
            var vault = new PasswordVault();
            var cred = vault.Retrieve(key, UserName);
            cred.RetrievePassword();
            return Task.FromResult<string?>(cred.Password);
        }
        catch
        {
            return Task.FromResult<string?>(null);
        }
    }

    public Task RemoveAsync(string key)
    {
        if (string.IsNullOrWhiteSpace(key)) throw new ArgumentException("Key is required.", nameof(key));

        try
        {
            var vault = new PasswordVault();
            var cred = vault.Retrieve(key, UserName);
            vault.Remove(cred);
        }
        catch
        {
            // ignore
        }

        return Task.CompletedTask;
    }
}
#endif
