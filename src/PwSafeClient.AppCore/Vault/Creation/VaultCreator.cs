using System;
using System.IO;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

using Medo.Security.Cryptography.PasswordSafe;

using PwSafeClient.AppCore.Configuration;

namespace PwSafeClient.AppCore.Vault.Creation;

public sealed class VaultCreator : IVaultCreator
{
    private static readonly Regex InvalidFileChars = new($"[{Regex.Escape(new string(Path.GetInvalidFileNameChars()))}]", RegexOptions.Compiled);

    private readonly IAppPaths _paths;

    public VaultCreator(IAppPaths paths)
    {
        _paths = paths;
    }

    public Task<(string Alias, string FilePath)> CreateNewAsync(
        string databaseName,
        string masterPassword,
        string? destinationFilePath = null,
        CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        if (string.IsNullOrWhiteSpace(databaseName))
        {
            throw new ArgumentException("Database name is required.", nameof(databaseName));
        }

        if (string.IsNullOrWhiteSpace(masterPassword))
        {
            throw new ArgumentException("Master password is required.", nameof(masterPassword));
        }

        var alias = databaseName.Trim();
        var finalPath = string.IsNullOrWhiteSpace(destinationFilePath)
            ? CreatePathInAppData(alias)
            : CreatePathFromDestination(destinationFilePath);

        var document = new Document(masterPassword);
        document.Save(finalPath);

        return Task.FromResult((alias, finalPath));
    }

    private string CreatePathInAppData(string alias)
    {
        var safeName = MakeSafeFileName(alias);

        var root = _paths.AppDataDirectory;
        if (string.IsNullOrWhiteSpace(root))
        {
            throw new InvalidOperationException("AppDataDirectory is not available.");
        }

        var vaultFolder = Path.Combine(root, "pwsafe", "vaults");
        Directory.CreateDirectory(vaultFolder);

        var basePath = Path.Combine(vaultFolder, $"{safeName}.psafe3");
        return EnsureUniquePath(basePath);
    }

    private static string CreatePathFromDestination(string destinationFilePath)
    {
        var trimmed = destinationFilePath.Trim();

        var ext = Path.GetExtension(trimmed);
        if (string.IsNullOrWhiteSpace(ext))
        {
            trimmed += ".psafe3";
        }

        var folder = Path.GetDirectoryName(trimmed);
        if (string.IsNullOrWhiteSpace(folder))
        {
            throw new ArgumentException("Destination path is invalid.", nameof(destinationFilePath));
        }

        Directory.CreateDirectory(folder);
        return EnsureUniquePath(trimmed);
    }

    private static string MakeSafeFileName(string name)
    {
        var trimmed = name.Trim();
        var noInvalid = InvalidFileChars.Replace(trimmed, "_");
        return string.IsNullOrWhiteSpace(noInvalid) ? "vault" : noInvalid;
    }

    private static string EnsureUniquePath(string path)
    {
        if (!File.Exists(path))
        {
            return path;
        }

        var folder = Path.GetDirectoryName(path) ?? string.Empty;
        var fileName = Path.GetFileNameWithoutExtension(path);
        var ext = Path.GetExtension(path);

        for (var i = 1; i < 1000; i++)
        {
            var candidate = Path.Combine(folder, $"{fileName} ({i}){ext}");
            if (!File.Exists(candidate))
            {
                return candidate;
            }
        }

        throw new IOException("Unable to create a unique database file name.");
    }
}
