namespace PwSafeClient.AppCore.Security.Biometrics;

public sealed record BiometricAuthResult(BiometricAuthStatus Status, string? ErrorMessage = null)
{
    public bool IsSuccess => Status == BiometricAuthStatus.Succeeded;

    public static BiometricAuthResult Success() => new(BiometricAuthStatus.Succeeded);

    public static BiometricAuthResult Fail(BiometricAuthStatus status, string? message = null)
        => new(status, message);
}
