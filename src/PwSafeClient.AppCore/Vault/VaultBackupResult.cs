namespace PwSafeClient.AppCore.Vault;

public sealed record VaultBackupResult(bool IsSuccess, string? BackupFilePath, string? ErrorMessage)
{
    public static VaultBackupResult Success(string backupFilePath) => new(true, backupFilePath, null);

    public static VaultBackupResult Fail(string message) => new(false, null, message);
}
