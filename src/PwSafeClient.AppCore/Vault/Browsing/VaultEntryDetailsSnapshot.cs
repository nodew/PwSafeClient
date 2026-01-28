namespace PwSafeClient.AppCore.Vault.Browsing;

public sealed class VaultEntryDetailsSnapshot
{
    public required string Title { get; init; }
    public string? UserName { get; init; }
    public string? Password { get; init; }
    public string? Url { get; init; }
    public string? Notes { get; init; }
    public string? GroupPath { get; init; }
    public string? PasswordPolicyName { get; init; }
}
