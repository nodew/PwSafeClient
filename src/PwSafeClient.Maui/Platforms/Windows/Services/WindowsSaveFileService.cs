#if WINDOWS
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.Maui.Platform;

using Windows.Storage.Pickers;

namespace PwSafeClient.Maui.Platforms.Windows.Services;

public sealed class WindowsSaveFileService : PwSafeClient.Maui.Services.ISaveFileService
{
    public async Task<string?> PickSaveFilePathAsync(string suggestedFileName, CancellationToken cancellationToken = default)
    {
        cancellationToken.ThrowIfCancellationRequested();

        var window = Application.Current?.Windows.FirstOrDefault();
        if (window?.Handler?.PlatformView is not MauiWinUIWindow platformWindow)
        {
            return null;
        }

        var safeFileName = MakeSafeFileName(suggestedFileName);
        if (string.IsNullOrWhiteSpace(safeFileName))
        {
            safeFileName = "vault";
        }

        var picker = new FileSavePicker
        {
            SuggestedFileName = safeFileName,
            SuggestedStartLocation = PickerLocationId.DocumentsLibrary,
        };

        picker.FileTypeChoices.Add("Password Safe", new List<string> { ".psafe3" });

        var hwnd = WinRT.Interop.WindowNative.GetWindowHandle(platformWindow);
        WinRT.Interop.InitializeWithWindow.Initialize(picker, hwnd);

        var file = await picker.PickSaveFileAsync();
        return file?.Path;
    }

    private static string MakeSafeFileName(string name)
    {
        var trimmed = (name ?? string.Empty).Trim();
        if (trimmed.Length == 0)
        {
            return string.Empty;
        }

        var invalid = Path.GetInvalidFileNameChars();
        var cleaned = new string(trimmed.Select(ch => invalid.Contains(ch) ? '_' : ch).ToArray());
        return cleaned.Trim();
    }
}
#endif
