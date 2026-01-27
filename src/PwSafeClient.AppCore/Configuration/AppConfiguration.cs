using System.Collections.Generic;

namespace PwSafeClient.AppCore.Configuration;

public sealed class AppConfiguration
{
    public Dictionary<string, string> Databases { get; set; } = new();

    public string? DefaultDatabase { get; set; }

    public Dictionary<string, bool> DatabaseSyncStates { get; set; } = new();

    public int MaxBackupCount { get; set; } = 10;

    public int ClipboardClearSeconds { get; set; } = 30;

    public int AutoLockMinutes { get; set; } = 5;

    public bool IsBiometricUnlockEnabled { get; set; } = false;

    public string Language { get; set; } = "en";

    public AppThemePreference Theme { get; set; } = AppThemePreference.System;

    public CloudSyncProvider CloudSyncProvider { get; set; } = CloudSyncProvider.None;

    public string? CloudSyncAccount { get; set; }

    public DateTimeOffset? CloudSyncLastSyncedAt { get; set; }

    public bool CloudSyncOnSave { get; set; } = true;

    public CloudSyncSchedule CloudSyncSchedule { get; set; } = CloudSyncSchedule.Daily;

    public bool CloudSyncOnCellular { get; set; } = false;
}
