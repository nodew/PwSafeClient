namespace PwSafeClient.AppCore.Security.Biometrics;

public enum BiometricAuthStatus
{
    Unknown = 0,
    Succeeded = 1,
    Failed = 2,
    Canceled = 3,
    NotAvailable = 4,
    NotConfigured = 5,
    DisabledByPolicy = 6,
    Busy = 7
}
