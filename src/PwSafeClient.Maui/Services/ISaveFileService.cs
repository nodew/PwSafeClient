namespace PwSafeClient.Maui.Services;

public interface ISaveFileService
{
    Task<string?> PickSaveFilePathAsync(
        string suggestedFileName,
        CancellationToken cancellationToken = default);
}
