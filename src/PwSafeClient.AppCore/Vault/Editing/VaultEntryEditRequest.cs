namespace PwSafeClient.AppCore.Vault.Editing;

public sealed class VaultEntryEditRequest
{
    public required string Title { get; init; }
    public string? UserName { get; init; }
    public string? Password { get; init; }
    public string? PasswordPolicyName { get; init; }
    public string? Url { get; init; }
    public string? Notes { get; init; }
    public string? GroupPath { get; init; }
}
