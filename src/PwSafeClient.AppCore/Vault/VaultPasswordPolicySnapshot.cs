using Medo.Security.Cryptography.PasswordSafe;

namespace PwSafeClient.AppCore.Vault;

public sealed class VaultPasswordPolicySnapshot
{
    public required string Name { get; init; }
    public int TotalPasswordLength { get; init; }
    public int MinimumLowercaseCount { get; init; }
    public int MinimumUppercaseCount { get; init; }
    public int MinimumDigitCount { get; init; }
    public int MinimumSymbolCount { get; init; }
    public PasswordPolicyStyle Style { get; init; }
    public string SymbolSet { get; init; } = string.Empty;
}
