namespace PwSafeClient.AppCore.CloudSync;

public enum CloudSyncStatus
{
    NotConfigured = 0,
    Ready = 1,
    Syncing = 2,
    SyncFailed = 3,
    Disconnected = 4
}
