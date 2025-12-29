namespace PwSafeClient.AppCore.Vault;

public sealed record VaultChangePassphraseResult(bool IsSuccess, string? ErrorMessage)
{
    public static VaultChangePassphraseResult Success() => new(true, null);

    public static VaultChangePassphraseResult Fail(string message) => new(false, message);
}
