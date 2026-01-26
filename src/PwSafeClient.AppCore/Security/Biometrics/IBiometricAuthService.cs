using System.Threading.Tasks;

namespace PwSafeClient.AppCore.Security.Biometrics;

public interface IBiometricAuthService
{
    Task<BiometricAvailability> GetAvailabilityAsync();

    Task<BiometricAuthResult> AuthenticateAsync(string reason);
}
