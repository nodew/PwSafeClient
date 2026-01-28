namespace PwSafeClient.Maui.Services;

public interface IFilePickerService
{
    Task<string?> PickDatabaseFileAsync(CancellationToken cancellationToken = default);
    Task<string?> PickPolicyFileAsync(CancellationToken cancellationToken = default);
}
