namespace PwSafeClient.AppCore.Vault.Editing;

public sealed record VaultEntryDeleteResult(bool IsSuccess, string? ErrorMessage)
{
    public static VaultEntryDeleteResult Success() => new(true, null);

    public static VaultEntryDeleteResult Fail(string message) => new(false, message);
}
