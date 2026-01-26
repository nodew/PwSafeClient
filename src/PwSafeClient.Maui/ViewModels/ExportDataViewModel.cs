using System.Text;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.Vault;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class ExportDataViewModel : ObservableObject
{
    private readonly IVaultSession _vaultSession;

    public ExportDataViewModel(IVaultSession vaultSession)
    {
        _vaultSession = vaultSession;
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    [RelayCommand]
    private Task CloseAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task ExportCopyAsync()
    {
        ErrorMessage = null;

        var (path, folder, baseName) = GetVaultPathParts();
        if (path == null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var target = Path.Combine(folder!, $"{baseName}_export_{DateTime.Now:yyyyMMdd_HHmmss}.psafe3");
            File.Copy(path, target, overwrite: true);

            await ShareFileAsync(target);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportCsvAsync()
    {
        ErrorMessage = null;

        var (path, folder, baseName) = GetVaultPathParts();
        if (path == null)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var target = Path.Combine(folder!, $"{baseName}_export_{DateTime.Now:yyyyMMdd_HHmmss}.csv");
            var csv = BuildCsv();
            await File.WriteAllTextAsync(target, csv, Encoding.UTF8);

            await ShareFileAsync(target);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task ExportPlainTextAsync()
    {
        ErrorMessage = null;

        var (path, folder, baseName) = GetVaultPathParts();
        if (path == null)
        {
            return;
        }

            var confirm = Shell.Current != null && await Shell.Current.DisplayAlertAsync(
            "Export Plain Text",
            "This will export all entries including passwords in plain text. Continue?",
            "Export",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        IsBusy = true;

        try
        {
            var target = Path.Combine(folder!, $"{baseName}_export_{DateTime.Now:yyyyMMdd_HHmmss}.txt");
            var text = BuildPlainText();
            await File.WriteAllTextAsync(target, text, Encoding.UTF8);

            await ShareFileAsync(target);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private (string? Path, string? Folder, string? BaseName) GetVaultPathParts()
    {
        if (!_vaultSession.IsUnlocked)
        {
            ErrorMessage = "Unlock the vault first.";
            return (null, null, null);
        }

        var path = _vaultSession.CurrentFilePath;
        if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
        {
            ErrorMessage = "Current vault file not found.";
            return (null, null, null);
        }

        var folder = System.IO.Path.GetDirectoryName(path);
        var baseName = System.IO.Path.GetFileNameWithoutExtension(path);
        if (string.IsNullOrWhiteSpace(folder) || string.IsNullOrWhiteSpace(baseName))
        {
            ErrorMessage = "Unable to determine export path.";
            return (null, null, null);
        }

        return (path, folder, baseName);
    }

    private string BuildCsv()
    {
        var entries = _vaultSession.GetEntriesSnapshot();

        var sb = new StringBuilder();
        sb.AppendLine("Title,Username,Password,Url,Group,Notes");

        for (var i = 0; i < entries.Count; i++)
        {
            var d = _vaultSession.GetEntryDetailsSnapshot(i, includePassword: true);
            if (d == null)
            {
                continue;
            }

            sb.Append(Csv(d.Title)).Append(',');
            sb.Append(Csv(d.UserName)).Append(',');
            sb.Append(Csv(d.Password)).Append(',');
            sb.Append(Csv(d.Url)).Append(',');
            sb.Append(Csv(d.GroupPath)).Append(',');
            sb.Append(Csv(d.Notes)).AppendLine();
        }

        return sb.ToString();
    }

    private string BuildPlainText()
    {
        var entries = _vaultSession.GetEntriesSnapshot();
        var sb = new StringBuilder();

        for (var i = 0; i < entries.Count; i++)
        {
            var d = _vaultSession.GetEntryDetailsSnapshot(i, includePassword: true);
            if (d == null)
            {
                continue;
            }

            sb.AppendLine($"Title: {d.Title}");
            if (!string.IsNullOrWhiteSpace(d.GroupPath)) sb.AppendLine($"Group: {d.GroupPath}");
            if (!string.IsNullOrWhiteSpace(d.UserName)) sb.AppendLine($"Username: {d.UserName}");
            if (!string.IsNullOrWhiteSpace(d.Password)) sb.AppendLine($"Password: {d.Password}");
            if (!string.IsNullOrWhiteSpace(d.Url)) sb.AppendLine($"Url: {d.Url}");
            if (!string.IsNullOrWhiteSpace(d.Notes)) sb.AppendLine($"Notes: {d.Notes}");
            sb.AppendLine(new string('-', 24));
        }

        return sb.ToString();
    }

    private static string Csv(string? value)
    {
        var s = value ?? string.Empty;
        var needsQuotes = s.Contains(',') || s.Contains('"') || s.Contains('\n') || s.Contains('\r');
        if (!needsQuotes)
        {
            return s;
        }

        return "\"" + s.Replace("\"", "\"\"") + "\"";
    }

    private static async Task ShareFileAsync(string fullPath)
    {
        try
        {
            await Share.Default.RequestAsync(new ShareFileRequest
            {
                Title = "Export",
                File = new ShareFile(fullPath)
            });
        }
        catch
        {
            // ignore share failures (e.g., unsupported platform)
        }
    }
}
