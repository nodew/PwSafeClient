#if WINDOWS
using System;
using System.Threading.Tasks;

using PwSafeClient.AppCore.Security.Biometrics;

using Windows.Security.Credentials.UI;

namespace PwSafeClient.Maui.Platforms.Windows.Services;

public sealed class WindowsHelloBiometricAuthService : IBiometricAuthService
{
    public async Task<BiometricAvailability> GetAvailabilityAsync()
    {
        try
        {
            var availability = await UserConsentVerifier.CheckAvailabilityAsync();
            return availability switch
            {
                UserConsentVerifierAvailability.Available => BiometricAvailability.Available,
                UserConsentVerifierAvailability.DeviceNotPresent => BiometricAvailability.NotAvailable,
                UserConsentVerifierAvailability.NotConfiguredForUser => BiometricAvailability.NotConfigured,
                UserConsentVerifierAvailability.DisabledByPolicy => BiometricAvailability.DisabledByPolicy,
                UserConsentVerifierAvailability.DeviceBusy => BiometricAvailability.Busy,
                _ => BiometricAvailability.Unknown
            };
        }
        catch
        {
            return BiometricAvailability.Unknown;
        }
    }

    public async Task<BiometricAuthResult> AuthenticateAsync(string reason)
    {
        try
        {
            var result = await UserConsentVerifier.RequestVerificationAsync(string.IsNullOrWhiteSpace(reason) ? "Unlock" : reason);
            return result switch
            {
                UserConsentVerificationResult.Verified => BiometricAuthResult.Success(),
                UserConsentVerificationResult.Canceled => BiometricAuthResult.Fail(BiometricAuthStatus.Canceled, "Canceled."),
                UserConsentVerificationResult.DeviceNotPresent => BiometricAuthResult.Fail(BiometricAuthStatus.NotAvailable, "Windows Hello device not present."),
                UserConsentVerificationResult.NotConfiguredForUser => BiometricAuthResult.Fail(BiometricAuthStatus.NotConfigured, "Windows Hello is not configured."),
                UserConsentVerificationResult.DisabledByPolicy => BiometricAuthResult.Fail(BiometricAuthStatus.DisabledByPolicy, "Windows Hello is disabled by policy."),
                UserConsentVerificationResult.DeviceBusy => BiometricAuthResult.Fail(BiometricAuthStatus.Busy, "Windows Hello device is busy."),
                UserConsentVerificationResult.RetriesExhausted => BiometricAuthResult.Fail(BiometricAuthStatus.Failed, "Too many attempts."),
                _ => BiometricAuthResult.Fail(BiometricAuthStatus.Failed, "Authentication failed.")
            };
        }
        catch (Exception ex)
        {
            return BiometricAuthResult.Fail(BiometricAuthStatus.Unknown, ex.Message);
        }
    }
}
#endif
