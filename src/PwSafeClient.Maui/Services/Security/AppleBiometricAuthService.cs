#if IOS || MACCATALYST
using System;
using System.Threading.Tasks;

using Foundation;
using LocalAuthentication;

using PwSafeClient.AppCore.Security.Biometrics;

namespace PwSafeClient.Maui.Services.Security;

public sealed class AppleBiometricAuthService : IBiometricAuthService
{
    public Task<BiometricAvailability> GetAvailabilityAsync()
    {
        try
        {
            using var ctx = new LAContext();

            NSError? error;
            var ok = ctx.CanEvaluatePolicy(LAPolicy.DeviceOwnerAuthenticationWithBiometrics, out error);
            if (ok)
            {
                return Task.FromResult(BiometricAvailability.Available);
            }

            return Task.FromResult(MapAvailability(error));
        }
        catch
        {
            return Task.FromResult(BiometricAvailability.Unknown);
        }
    }

    public Task<BiometricAuthResult> AuthenticateAsync(string reason)
    {
        var tcs = new TaskCompletionSource<BiometricAuthResult>();

        try
        {
            var ctx = new LAContext();

            ctx.EvaluatePolicy(
                LAPolicy.DeviceOwnerAuthenticationWithBiometrics,
                string.IsNullOrWhiteSpace(reason) ? "Authenticate" : reason,
                (success, error) =>
                {
                    try
                    {
                        if (success)
                        {
                            tcs.TrySetResult(BiometricAuthResult.Success());
                            return;
                        }

                        tcs.TrySetResult(MapAuthFailure(error));
                    }
                    finally
                    {
                        ctx.Dispose();
                    }
                });
        }
        catch (Exception ex)
        {
            tcs.TrySetResult(BiometricAuthResult.Fail(BiometricAuthStatus.Failed, ex.Message));
        }

        return tcs.Task;
    }

    private static BiometricAvailability MapAvailability(NSError? error)
    {
        if (error == null)
        {
            return BiometricAvailability.NotAvailable;
        }

        var code = (LAStatus)(long)error.Code;
        return code switch
        {
            LAStatus.BiometryNotAvailable => BiometricAvailability.NotAvailable,
            LAStatus.BiometryNotEnrolled => BiometricAvailability.NotConfigured,
            LAStatus.PasscodeNotSet => BiometricAvailability.NotConfigured,
            LAStatus.BiometryLockout => BiometricAvailability.Busy,
            _ => BiometricAvailability.NotAvailable
        };
    }

    private static BiometricAuthResult MapAuthFailure(NSError? error)
    {
        if (error == null)
        {
            return BiometricAuthResult.Fail(BiometricAuthStatus.Failed, "Authentication failed.");
        }

        var code = (LAStatus)(long)error.Code;

        return code switch
        {
            LAStatus.UserCancel => BiometricAuthResult.Fail(BiometricAuthStatus.Canceled, "Canceled."),
            LAStatus.SystemCancel => BiometricAuthResult.Fail(BiometricAuthStatus.Canceled, "Canceled."),
            LAStatus.AppCancel => BiometricAuthResult.Fail(BiometricAuthStatus.Canceled, "Canceled."),
            LAStatus.UserFallback => BiometricAuthResult.Fail(BiometricAuthStatus.Canceled, "Canceled."),
            LAStatus.AuthenticationFailed => BiometricAuthResult.Fail(BiometricAuthStatus.Failed, "Authentication failed."),
            LAStatus.BiometryNotEnrolled => BiometricAuthResult.Fail(BiometricAuthStatus.NotConfigured, "Biometrics not enrolled."),
            LAStatus.BiometryNotAvailable => BiometricAuthResult.Fail(BiometricAuthStatus.NotAvailable, "Biometrics not available."),
            LAStatus.BiometryLockout => BiometricAuthResult.Fail(BiometricAuthStatus.Busy, "Biometrics locked. Try again later."),
            LAStatus.PasscodeNotSet => BiometricAuthResult.Fail(BiometricAuthStatus.NotConfigured, "Device passcode not set."),
            _ => BiometricAuthResult.Fail(BiometricAuthStatus.Failed, error.LocalizedDescription)
        };
    }
}
#endif
