using System;
using System.Security.Cryptography;
using System.Text;

namespace PwSafeClient.Maui.Services.Security;

internal static class BiometricSecretKeys
{
    private const string Prefix = "pwsafe:masterpwd:";

    public static string ForVaultFilePath(string filePath)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            throw new ArgumentException("File path is required.", nameof(filePath));
        }

        var normalized = filePath.Trim();
        var bytes = SHA256.HashData(Encoding.UTF8.GetBytes(normalized));
        var base64 = Convert.ToBase64String(bytes);

        // Keep key portable: remove padding and URL-unsafe characters.
        base64 = base64.TrimEnd('=')
            .Replace('+', '-')
            .Replace('/', '_');

        return Prefix + base64;
    }
}
