namespace PwSafeClient.AppCore.Vault;

public enum VaultLoadError
{
    None = 0,
    FileNotFound,
    InvalidPasswordOrInvalidFormat,
    Unknown
}
