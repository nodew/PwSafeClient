using PwSafeClient.AppCore.Configuration;

namespace PwSafeClient.AppCore.CloudSync;

public sealed record CloudSyncState(
    CloudSyncProvider Provider,
    string? AccountName,
    CloudSyncStatus Status,
    DateTimeOffset? LastSyncedAt,
    CloudSyncSchedule Schedule,
    bool SyncOnSave,
    bool SyncOnCellular);
