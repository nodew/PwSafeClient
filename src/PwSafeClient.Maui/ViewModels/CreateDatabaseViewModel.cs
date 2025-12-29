using System;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Databases;
using PwSafeClient.AppCore.Security.Secrets;
using PwSafeClient.AppCore.Vault;
using PwSafeClient.AppCore.Vault.Creation;
using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.Services.Security;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class CreateDatabaseViewModel : ObservableObject
{
    private readonly IVaultCreator _vaultCreator;
    private readonly IDatabaseRegistry _databaseRegistry;
    private readonly IVaultSession _vaultSession;
    private readonly AutoLockService _autoLock;
    private readonly IAppConfigurationStore _configStore;
    private readonly ISecureSecretStore _secretStore;
    private readonly ISaveFileService _saveFileService;

    public CreateDatabaseViewModel(
        IVaultCreator vaultCreator,
        IDatabaseRegistry databaseRegistry,
        IVaultSession vaultSession,
        AutoLockService autoLock,
        IAppConfigurationStore configStore,
        ISecureSecretStore secretStore,
        ISaveFileService saveFileService)
    {
        _vaultCreator = vaultCreator;
        _databaseRegistry = databaseRegistry;
        _vaultSession = vaultSession;
        _autoLock = autoLock;
        _configStore = configStore;
        _secretStore = secretStore;
        _saveFileService = saveFileService;
    }

    private string _databaseName = string.Empty;
    public string DatabaseName
    {
        get => _databaseName;
        set => SetProperty(ref _databaseName, value);
    }

    private string _masterPassword = string.Empty;
    public string MasterPassword
    {
        get => _masterPassword;
        set => SetProperty(ref _masterPassword, value);
    }

    private string _confirmPassword = string.Empty;
    public string ConfirmPassword
    {
        get => _confirmPassword;
        set => SetProperty(ref _confirmPassword, value);
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
    private Task CancelAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private async Task CreateAsync()
    {
        ErrorMessage = null;

        var name = DatabaseName?.Trim() ?? string.Empty;
        if (string.IsNullOrWhiteSpace(name))
        {
            ErrorMessage = "Database name is required.";
            return;
        }

        if (string.IsNullOrEmpty(MasterPassword))
        {
            ErrorMessage = "Master password is required.";
            return;
        }

        if (!string.Equals(MasterPassword, ConfirmPassword, StringComparison.Ordinal))
        {
            ErrorMessage = "Passwords do not match.";
            return;
        }

        IsBusy = true;

        try
        {
            var destinationFilePath = await _saveFileService.PickSaveFilePathAsync(name);
            var (alias, filePath) = await _vaultCreator.CreateNewAsync(name, MasterPassword, destinationFilePath);

            await _databaseRegistry.AddOrUpdateAsync(alias, filePath, makeDefault: true);

            var load = await _vaultSession.LoadAsync(filePath, MasterPassword, readOnly: false);
            if (!load.IsSuccess)
            {
                ErrorMessage = load.ErrorMessage;
                return;
            }

            _autoLock.NotifyVaultUnlocked(filePath);

            await SaveUnlockSecretIfEnabledAsync(filePath, MasterPassword);

            await Shell.Current.GoToAsync(Routes.Vault);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            MasterPassword = string.Empty;
            ConfirmPassword = string.Empty;
        }
    }

    private async Task SaveUnlockSecretIfEnabledAsync(string filePath, string masterPassword)
    {
        if (string.IsNullOrWhiteSpace(filePath) || string.IsNullOrEmpty(masterPassword))
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

        if (!cfg.IsBiometricUnlockEnabled)
        {
            return;
        }

        try
        {
            var key = BiometricSecretKeys.ForVaultFilePath(filePath);
            await _secretStore.SaveAsync(key, masterPassword);
        }
        catch
        {
            // ignore secure store failures
        }
    }
}
