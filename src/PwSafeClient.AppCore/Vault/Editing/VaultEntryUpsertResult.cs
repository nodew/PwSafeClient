namespace PwSafeClient.AppCore.Vault.Editing;

public sealed class VaultEntryUpsertResult
{
    private VaultEntryUpsertResult(bool isSuccess, int? entryIndex, string? errorMessage)
    {
        IsSuccess = isSuccess;
        EntryIndex = entryIndex;
        ErrorMessage = errorMessage;
    }

    public bool IsSuccess { get; }
    public int? EntryIndex { get; }
    public string? ErrorMessage { get; }

    public static VaultEntryUpsertResult Success(int entryIndex) => new(true, entryIndex, null);
    public static VaultEntryUpsertResult Fail(string errorMessage) => new(false, null, errorMessage);
}
