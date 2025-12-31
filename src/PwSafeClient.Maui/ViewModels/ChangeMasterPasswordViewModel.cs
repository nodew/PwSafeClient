using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Security.Secrets;
using PwSafeClient.AppCore.Vault;
using PwSafeClient.Maui.Services.Security;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class ChangeMasterPasswordViewModel : ObservableObject
{
    private readonly IVaultSession _vaultSession;
    private readonly IAppConfigurationStore _configStore;
    private readonly ISecureSecretStore _secretStore;

    public ChangeMasterPasswordViewModel(
        IVaultSession vaultSession,
        IAppConfigurationStore configStore,
        ISecureSecretStore secretStore)
    {
        _vaultSession = vaultSession;
        _configStore = configStore;
        _secretStore = secretStore;
    }

    private string _currentPassword = string.Empty;
    public string CurrentPassword
    {
        get => _currentPassword;
        set => SetProperty(ref _currentPassword, value);
    }

    private string _newPassword = string.Empty;
    public string NewPassword
    {
        get => _newPassword;
        set => SetProperty(ref _newPassword, value);
    }

    private string _confirmNewPassword = string.Empty;
    public string ConfirmNewPassword
    {
        get => _confirmNewPassword;
        set => SetProperty(ref _confirmNewPassword, value);
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
    private async Task ChangeAsync()
    {
        ErrorMessage = null;

        if (!_vaultSession.IsUnlocked)
        {
            ErrorMessage = "Unlock the vault first.";
            return;
        }

        if (string.IsNullOrEmpty(CurrentPassword))
        {
            ErrorMessage = "Current password is required.";
            return;
        }

        if (string.IsNullOrEmpty(NewPassword))
        {
            ErrorMessage = "New password is required.";
            return;
        }

        if (!string.Equals(NewPassword, ConfirmNewPassword, StringComparison.Ordinal))
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _vaultSession.ChangePassphraseAsync(CurrentPassword, NewPassword);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            await UpdateStoredSecretIfNeededAsync(_vaultSession.CurrentFilePath, NewPassword);

            var shell = Shell.Current;
            if (shell != null)
            {
                await shell.DisplayAlertAsync("Master Password", "Master password updated.", "OK");
                await shell.GoToAsync("..");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            CurrentPassword = string.Empty;
            NewPassword = string.Empty;
            ConfirmNewPassword = string.Empty;
        }
    }

    private async Task UpdateStoredSecretIfNeededAsync(string? filePath, string newPassword)
    {
        if (string.IsNullOrWhiteSpace(filePath))
        {
            return;
        }

        AppConfiguration cfg;
        try
        {
            cfg = await _configStore.LoadAsync();
        }
        catch
        {
            return;
        }

        var key = BiometricSecretKeys.ForVaultFilePath(filePath);

        try
        {
            if (cfg.IsBiometricUnlockEnabled)
            {
                await _secretStore.SaveAsync(key, newPassword);
            }
            else
            {
                await _secretStore.RemoveAsync(key);
            }
        }
        catch
        {
            // ignore secure store failures
        }
    }
}
