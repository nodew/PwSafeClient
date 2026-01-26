namespace PwSafeClient.AppCore.Vault;

public sealed record VaultLoadResult(
    bool IsSuccess,
    VaultLoadError Error,
    string? ErrorMessage)
{
    public static VaultLoadResult Success() => new(true, VaultLoadError.None, null);

    public static VaultLoadResult Fail(VaultLoadError error, string message) => new(false, error, message);
}
