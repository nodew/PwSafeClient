using System;
using System.IO;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Databases;
using PwSafeClient.AppCore.Security.Biometrics;
using PwSafeClient.AppCore.Security.Secrets;
using PwSafeClient.AppCore.Vault;
using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.Services.Security;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class UnlockViewModel : ObservableObject
{
    private readonly IVaultSession _vaultSession;
    private readonly IDatabaseRegistry _databaseRegistry;
    private readonly AutoLockService _autoLock;
    private readonly IAppConfigurationStore _configStore;
    private readonly ISecureSecretStore _secretStore;
    private readonly IBiometricAuthService _biometricAuth;

    public UnlockViewModel(
        IVaultSession vaultSession,
        IDatabaseRegistry databaseRegistry,
        AutoLockService autoLock,
        IAppConfigurationStore configStore,
        ISecureSecretStore secretStore,
        IBiometricAuthService biometricAuth)
    {
        _vaultSession = vaultSession;
        _databaseRegistry = databaseRegistry;
        _autoLock = autoLock;
        _configStore = configStore;
        _secretStore = secretStore;
        _biometricAuth = biometricAuth;
    }

    private bool _isBiometricUnlockVisible;
    public bool IsBiometricUnlockVisible
    {
        get => _isBiometricUnlockVisible;
        private set => SetProperty(ref _isBiometricUnlockVisible, value);
    }

    public string BiometricButtonText
    {
        get
        {
            return DeviceInfo.Platform == DevicePlatform.WinUI
                ? "Use Windows Hello"
                : "Use biometrics";
        }
    }

    private string? _filePath;
    public string? FilePath
    {
        get => _filePath;
        private set => SetProperty(ref _filePath, value);
    }

    private string? _displayName;
    public string? DisplayName
    {
        get => _displayName;
        private set => SetProperty(ref _displayName, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        private set => SetProperty(ref _isBusy, value);
    }

    public void SetFilePath(string? filePath)
    {
        Alias = null;
        FilePath = filePath;
        DisplayName = string.IsNullOrWhiteSpace(filePath) ? null : Path.GetFileName(filePath);
        ErrorMessage = null;
        Password = string.Empty;

        _ = RefreshBiometricStateAsync();
    }

    private string? _alias;
    public string? Alias
    {
        get => _alias;
        private set => SetProperty(ref _alias, value);
    }

    public async Task SetAliasAsync(string? alias)
    {
        Alias = alias;
        ErrorMessage = null;
        Password = string.Empty;

        if (string.IsNullOrWhiteSpace(alias))
        {
            SetFilePath(null);
            return;
        }

        var path = await _databaseRegistry.TryGetPathAsync(alias);
        FilePath = path;
        DisplayName = alias;

        await RefreshBiometricStateAsync();
    }

    private async Task RefreshBiometricStateAsync()
    {
        IsBiometricUnlockVisible = false;
        OnPropertyChanged(nameof(BiometricButtonText));

        var filePath = FilePath;
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

        if (!cfg.IsBiometricUnlockEnabled)
        {
            return;
        }

        var availability = await _biometricAuth.GetAvailabilityAsync();
        if (availability != BiometricAvailability.Available)
        {
            return;
        }

        var key = BiometricSecretKeys.ForVaultFilePath(filePath);
        var secret = await _secretStore.TryGetAsync(key);
        if (!string.IsNullOrEmpty(secret))
        {
            IsBiometricUnlockVisible = true;
        }
    }

    [RelayCommand]
    private async Task UnlockAsync()
    {
        ErrorMessage = null;

        if (string.IsNullOrWhiteSpace(FilePath))
        {
            ErrorMessage = "Database not found.";
            return;
        }

        if (string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Password is required.";
            return;
        }

        IsBusy = true;

        try
        {
            var result = await _vaultSession.LoadAsync(FilePath, Password, readOnly: false);

            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            _autoLock.NotifyVaultUnlocked(FilePath);

            await SaveUnlockSecretIfEnabledAsync(FilePath, Password);

            await Shell.Current.GoToAsync($"{Routes.Vault}");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            Password = string.Empty;
        }
    }

    [RelayCommand]
    private async Task BiometricUnlockAsync()
    {
        ErrorMessage = null;

        var filePath = FilePath;
        if (string.IsNullOrWhiteSpace(filePath))
        {
            ErrorMessage = "Database not found.";
            return;
        }

        IsBusy = true;

        try
        {
            var cfg = await _configStore.LoadAsync();
            if (!cfg.IsBiometricUnlockEnabled)
            {
                IsBiometricUnlockVisible = false;
                ErrorMessage = "Biometric unlock is disabled.";
                return;
            }

            var availability = await _biometricAuth.GetAvailabilityAsync();
            if (availability != BiometricAvailability.Available)
            {
                IsBiometricUnlockVisible = false;
                ErrorMessage = "Biometric authentication is not available.";
                return;
            }

            var auth = await _biometricAuth.AuthenticateAsync("Unlock your vault");
            if (!auth.IsSuccess)
            {
                // User can still fall back to manual password entry.
                ErrorMessage = auth.ErrorMessage ?? "Authentication failed.";
                return;
            }

            var key = BiometricSecretKeys.ForVaultFilePath(filePath);
            var secret = await _secretStore.TryGetAsync(key);
            if (string.IsNullOrEmpty(secret))
            {
                IsBiometricUnlockVisible = false;
                ErrorMessage = "No saved unlock secret found. Please enter your master password.";
                return;
            }

            var result = await _vaultSession.LoadAsync(filePath, secret, readOnly: false);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage;
                return;
            }

            _autoLock.NotifyVaultUnlocked(filePath);

            await Shell.Current.GoToAsync($"{Routes.Vault}");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
            Password = string.Empty;
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
            IsBiometricUnlockVisible = true;
        }
        catch
        {
            // Ignore secure store failures; user can still unlock manually.
        }
    }

    [RelayCommand]
    private Task SwitchDatabaseAsync()
    {
        return Shell.Current.GoToAsync($"//{Routes.DatabaseList}");
    }
}
