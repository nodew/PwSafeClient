using System.Collections.Generic;

namespace PwSafeClient.AppCore.Configuration;

public sealed class AppConfiguration
{
    public Dictionary<string, string> Databases { get; set; } = new();

    public string? DefaultDatabase { get; set; }

    public int MaxBackupCount { get; set; } = 10;

    public int ClipboardClearSeconds { get; set; } = 30;

    public int AutoLockMinutes { get; set; } = 5;

    public bool IsBiometricUnlockEnabled { get; set; } = false;

    public string Language { get; set; } = "en";

    public AppThemePreference Theme { get; set; } = AppThemePreference.System;
}
