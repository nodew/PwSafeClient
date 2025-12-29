namespace PwSafeClient.AppCore.Security.Biometrics;

public enum BiometricAvailability
{
    Unknown = 0,
    Available = 1,
    NotAvailable = 2,
    NotConfigured = 3,
    DisabledByPolicy = 4,
    Busy = 5
}
