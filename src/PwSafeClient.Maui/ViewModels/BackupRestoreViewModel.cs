using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.Vault;
using PwSafeClient.Maui.Services;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class BackupRestoreViewModel : ObservableObject
{
    private readonly IVaultSession _vaultSession;
    private readonly IFilePickerService _filePicker;

    public BackupRestoreViewModel(IVaultSession vaultSession, IFilePickerService filePicker)
    {
        _vaultSession = vaultSession;
        _filePicker = filePicker;
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
    private async Task CreateBackupAsync()
    {
        ErrorMessage = null;

        if (!_vaultSession.IsUnlocked)
        {
            ErrorMessage = "Unlock the vault first.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _vaultSession.CreateBackupAsync();
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            var shell = Shell.Current;
            if (shell != null)
            {
                await shell.DisplayAlertAsync("Backup", $"Backup created:\n{result.BackupFilePath}", "OK");
            }
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
    private async Task RestoreAsync()
    {
        ErrorMessage = null;

        if (!_vaultSession.IsUnlocked)
        {
            ErrorMessage = "Unlock the vault first.";
            return;
        }

        var currentPath = _vaultSession.CurrentFilePath;
        if (string.IsNullOrWhiteSpace(currentPath) || !File.Exists(currentPath))
        {
            ErrorMessage = "Current vault file not found.";
            return;
        }

        var restoreFrom = await _filePicker.PickDatabaseFileAsync();
        if (string.IsNullOrWhiteSpace(restoreFrom) || !File.Exists(restoreFrom))
        {
            return;
        }

        var shellForConfirm = Shell.Current;
        var confirm = shellForConfirm != null && await shellForConfirm.DisplayAlertAsync(
            "Restore",
            "This will overwrite your current database file. Continue?",
            "Restore",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        IsBusy = true;

        try
        {
            _vaultSession.Unload();

            File.Copy(restoreFrom, currentPath, overwrite: true);

            var encoded = Uri.EscapeDataString(currentPath);
            var shell = Shell.Current;
            if (shell != null)
            {
                await shell.GoToAsync($"//{Routes.DatabaseList}/{Routes.Unlock}?filePath={encoded}");
            }
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
}
