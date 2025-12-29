#if ANDROID
using System;
using System.Threading.Tasks;

using AndroidX.Biometric;
using AndroidX.Core.Content;
using AndroidX.Fragment.App;

using PwSafeClient.AppCore.Security.Biometrics;

namespace PwSafeClient.Maui.Platforms.Android.Services;

public sealed class AndroidBiometricAuthService : IBiometricAuthService
{
    public Task<BiometricAvailability> GetAvailabilityAsync()
    {
        try
        {
            var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
            if (activity == null)
            {
                return Task.FromResult(BiometricAvailability.Unknown);
            }

            var manager = BiometricManager.From(activity);
            var result = manager.CanAuthenticate(BiometricManager.Authenticators.BiometricStrong);

            return Task.FromResult(MapAvailability(result));
        }
        catch
        {
            return Task.FromResult(BiometricAvailability.Unknown);
        }
    }

    public Task<BiometricAuthResult> AuthenticateAsync(string reason)
    {
        var activity = Microsoft.Maui.ApplicationModel.Platform.CurrentActivity;
        if (activity is not FragmentActivity fragmentActivity)
        {
            return Task.FromResult(BiometricAuthResult.Fail(BiometricAuthStatus.NotAvailable, "Biometric authentication is not available."));
        }

        var tcs = new TaskCompletionSource<BiometricAuthResult>();

        fragmentActivity.RunOnUiThread(() =>
        {
            try
            {
                var executor = ContextCompat.GetMainExecutor(fragmentActivity);
                if (executor == null)
                {
                    tcs.TrySetResult(BiometricAuthResult.Fail(BiometricAuthStatus.NotAvailable, "Biometric authentication is not available."));
                    return;
                }

                var callback = new PromptCallback(tcs);
                var prompt = new BiometricPrompt(fragmentActivity, executor, callback);

                var promptInfo = new BiometricPrompt.PromptInfo.Builder()
                    .SetTitle(string.IsNullOrWhiteSpace(reason) ? "Authenticate" : reason)
                    .SetNegativeButtonText("Cancel")
                    .SetAllowedAuthenticators(BiometricManager.Authenticators.BiometricStrong)
                    .Build();

                prompt.Authenticate(promptInfo);
            }
            catch (Exception ex)
            {
                tcs.TrySetResult(BiometricAuthResult.Fail(BiometricAuthStatus.Failed, ex.Message));
            }
        });

        return tcs.Task;
    }

    private static BiometricAvailability MapAvailability(int canAuthenticate)
    {
        return canAuthenticate switch
        {
            BiometricManager.BiometricSuccess => BiometricAvailability.Available,
            BiometricManager.BiometricErrorNoneEnrolled => BiometricAvailability.NotConfigured,
            BiometricManager.BiometricErrorNoHardware => BiometricAvailability.NotAvailable,
            BiometricManager.BiometricErrorHwUnavailable => BiometricAvailability.Busy,
            BiometricManager.BiometricErrorSecurityUpdateRequired => BiometricAvailability.DisabledByPolicy,
            BiometricManager.BiometricErrorUnsupported => BiometricAvailability.NotAvailable,
            BiometricManager.BiometricStatusUnknown => BiometricAvailability.Unknown,
            _ => BiometricAvailability.Unknown
        };
    }

    private sealed class PromptCallback : BiometricPrompt.AuthenticationCallback
    {
        private readonly TaskCompletionSource<BiometricAuthResult> _tcs;

        public PromptCallback(TaskCompletionSource<BiometricAuthResult> tcs)
        {
            _tcs = tcs;
        }

        public override void OnAuthenticationSucceeded(BiometricPrompt.AuthenticationResult result)
        {
            _tcs.TrySetResult(BiometricAuthResult.Success());
        }

        public override void OnAuthenticationFailed()
        {
            _tcs.TrySetResult(BiometricAuthResult.Fail(BiometricAuthStatus.Failed, "Authentication failed."));
        }

        public override void OnAuthenticationError(int errorCode, Java.Lang.ICharSequence errString)
        {
            var message = errString?.ToString();

            var status = errorCode switch
            {
                BiometricPrompt.ErrorUserCanceled => BiometricAuthStatus.Canceled,
                BiometricPrompt.ErrorCanceled => BiometricAuthStatus.Canceled,
                BiometricPrompt.ErrorNegativeButton => BiometricAuthStatus.Canceled,
                BiometricPrompt.ErrorNoBiometrics => BiometricAuthStatus.NotConfigured,
                BiometricPrompt.ErrorHwNotPresent => BiometricAuthStatus.NotAvailable,
                BiometricPrompt.ErrorHwUnavailable => BiometricAuthStatus.Busy,
                BiometricPrompt.ErrorLockout => BiometricAuthStatus.Busy,
                BiometricPrompt.ErrorLockoutPermanent => BiometricAuthStatus.DisabledByPolicy,
                _ => BiometricAuthStatus.Failed
            };

            _tcs.TrySetResult(BiometricAuthResult.Fail(status, message));
        }
    }
}
#endif
