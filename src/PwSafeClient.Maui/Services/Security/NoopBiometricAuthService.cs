using System.Threading.Tasks;

using PwSafeClient.AppCore.Security.Biometrics;

namespace PwSafeClient.Maui.Services.Security;

public sealed class NoopBiometricAuthService : IBiometricAuthService
{
    public Task<BiometricAvailability> GetAvailabilityAsync()
        => Task.FromResult(BiometricAvailability.NotAvailable);

    public Task<BiometricAuthResult> AuthenticateAsync(string reason)
        => Task.FromResult(BiometricAuthResult.Fail(BiometricAuthStatus.NotAvailable, "Biometric authentication is not available."));
}
