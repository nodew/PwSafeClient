namespace PwSafeClient.AppCore.Vault.Editing;

public sealed record VaultGroupOperationResult(bool IsSuccess, string? ErrorMessage, int AffectedEntries)
{
    public static VaultGroupOperationResult Success(int affectedEntries = 0)
        => new(true, null, affectedEntries);

    public static VaultGroupOperationResult Fail(string message)
        => new(false, message, 0);
}
