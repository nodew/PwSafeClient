namespace PwSafeClient.AppCore.Vault.Browsing;

public sealed class VaultEntrySnapshot
{
    public required string Title { get; init; }
    public string? UserName { get; init; }
    public string? GroupPath { get; init; }
}
