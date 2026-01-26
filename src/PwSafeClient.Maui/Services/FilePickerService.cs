namespace PwSafeClient.Maui.Services;

public sealed class FilePickerService : IFilePickerService
{
    public async Task<string?> PickDatabaseFileAsync(CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var result = await FilePicker.Default.PickAsync(new PickOptions
        {
            PickerTitle = "Select a Password Safe database",
            FileTypes = GetDatabaseFileTypes()
        });

        cancellationToken.ThrowIfCancellationRequested();

        return result?.FullPath;
    }

    private static FilePickerFileType GetDatabaseFileTypes()
    {
        return new FilePickerFileType(new Dictionary<DevicePlatform, IEnumerable<string>>
        {
            // Keep permissive defaults on platforms where extension filters are not reliable.
            { DevicePlatform.iOS, new[] { "public.data" } },
            { DevicePlatform.macOS, new[] { "public.data" } },
            { DevicePlatform.Android, new[] { "application/octet-stream" } },
            { DevicePlatform.WinUI, new[] { ".psafe3", ".ibak" } },
        });
    }
}
