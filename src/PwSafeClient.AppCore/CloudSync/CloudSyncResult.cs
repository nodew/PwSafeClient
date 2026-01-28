namespace PwSafeClient.AppCore.CloudSync;

public sealed record CloudSyncResult(bool IsSuccess, bool HadConflict, string? ErrorMessage)
{
    public static CloudSyncResult Success(bool hadConflict = false) => new(true, hadConflict, null);
    public static CloudSyncResult Fail(string message) => new(false, false, message);
}
