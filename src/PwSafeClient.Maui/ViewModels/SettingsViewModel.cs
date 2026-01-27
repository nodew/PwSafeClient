using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using Microsoft.Maui.ApplicationModel;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Security.Secrets;
using PwSafeClient.AppCore.Vault;
using PwSafeClient.Maui.Services.Security;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IAppConfigurationStore _store;
    private readonly ISecureSecretStore _secretStore;
    private readonly IVaultSession _vaultSession;

    public SettingsViewModel(
        IAppConfigurationStore store,
        ISecureSecretStore secretStore,
        IVaultSession vaultSession)
    {
        _store = store;
        _secretStore = secretStore;
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

    private bool _isDatabaseOpen;
    public bool IsDatabaseOpen
    {
        get => _isDatabaseOpen;
        private set => SetProperty(ref _isDatabaseOpen, value);
    }

    private string _activeDatabaseName = "—";
    public string ActiveDatabaseName
    {
        get => _activeDatabaseName;
        private set => SetProperty(ref _activeDatabaseName, value);
    }

    private string _storageUsageDisplayName = "—";
    public string StorageUsageDisplayName
    {
        get => _storageUsageDisplayName;
        private set => SetProperty(ref _storageUsageDisplayName, value);
    }

    private string _appVersionDisplayName = string.Empty;
    public string AppVersionDisplayName
    {
        get => _appVersionDisplayName;
        private set => SetProperty(ref _appVersionDisplayName, value);
    }

    private AppThemePreference _theme;
    public AppThemePreference Theme
    {
        get => _theme;
        set
        {
            if (SetProperty(ref _theme, value))
            {
                ApplyThemeOverride();
                OnPropertyChanged(nameof(ThemeDisplayName));
            }
        }
    }

    public string ThemeDisplayName => Theme switch
    {
        AppThemePreference.System => "System",
        AppThemePreference.Light => "Light",
        AppThemePreference.Dark => "Dark",
        _ => "System"
    };

    private bool _isBiometricUnlockEnabled;
    public bool IsBiometricUnlockEnabled
    {
        get => _isBiometricUnlockEnabled;
        set => SetProperty(ref _isBiometricUnlockEnabled, value);
    }

    private bool _isDatabaseSyncEnabled;
    public bool IsDatabaseSyncEnabled
    {
        get => _isDatabaseSyncEnabled;
        set => SetProperty(ref _isDatabaseSyncEnabled, value);
    }

    private string _language = "en";
    public string Language
    {
        get => _language;
        set
        {
            if (SetProperty(ref _language, value))
            {
                OnPropertyChanged(nameof(LanguageDisplayName));
            }
        }
    }

    public string LanguageDisplayName => string.IsNullOrWhiteSpace(Language) ? "en" : Language.Trim();

    // Keep as strings for simple UI binding; validated/parsing on Save.
    private string _autoLockMinutes = "5";
    public string AutoLockMinutes
    {
        get => _autoLockMinutes;
        set
        {
            if (SetProperty(ref _autoLockMinutes, value))
            {
                OnPropertyChanged(nameof(AutoLockDisplayName));
            }
        }
    }

    public string AutoLockDisplayName => int.TryParse(AutoLockMinutes, out var v) ? $"{v} minutes" : "—";

    private string _clipboardClearSeconds = "30";
    public string ClipboardClearSeconds
    {
        get => _clipboardClearSeconds;
        set
        {
            if (SetProperty(ref _clipboardClearSeconds, value))
            {
                OnPropertyChanged(nameof(ClipboardClearDisplayName));
            }
        }
    }

    public string ClipboardClearDisplayName => int.TryParse(ClipboardClearSeconds, out var v) ? $"{v} seconds" : "—";

    private string _maxBackupCount = "10";
    public string MaxBackupCount
    {
        get => _maxBackupCount;
        set
        {
            if (SetProperty(ref _maxBackupCount, value))
            {
                OnPropertyChanged(nameof(MaxBackupCountDisplayName));
            }
        }
    }

    public string MaxBackupCountDisplayName => int.TryParse(MaxBackupCount, out var v) ? v.ToString() : "—";

    [RelayCommand]
    public async Task LoadAsync()
    {
        ErrorMessage = null;
        IsBusy = true;

        try
        {
            var config = await _store.LoadAsync();

            Theme = config.Theme;
            IsBiometricUnlockEnabled = config.IsBiometricUnlockEnabled;
            Language = string.IsNullOrWhiteSpace(config.Language) ? "en" : config.Language;

            AutoLockMinutes = config.AutoLockMinutes.ToString();
            ClipboardClearSeconds = config.ClipboardClearSeconds.ToString();
            MaxBackupCount = config.MaxBackupCount.ToString();

            UpdateDatabaseState(config);
            AppVersionDisplayName = BuildAppVersionDisplayName();
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
    private async Task SelectThemeAsync()
    {
        if (Shell.Current == null)
        {
            return;
        }

        var choice = await Shell.Current.DisplayActionSheetAsync(
            "Appearance",
            "Cancel",
            null,
            "System",
            "Light",
            "Dark");

        Theme = choice switch
        {
            "System" => AppThemePreference.System,
            "Light" => AppThemePreference.Light,
            "Dark" => AppThemePreference.Dark,
            _ => Theme
        };
    }

    [RelayCommand]
    private Task CloseAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private Task OpenChangeMasterPasswordAsync()
    {
        return Shell.Current.GoToAsync(Routes.ChangeMasterPassword);
    }

    [RelayCommand]
    private Task OpenBackupRestoreAsync()
    {
        return Shell.Current.GoToAsync(Routes.BackupRestore);
    }

    [RelayCommand]
    private Task OpenExportDataAsync()
    {
        return Shell.Current.GoToAsync(Routes.ExportData);
    }

    [RelayCommand]
    private Task OpenCloudSyncAsync()
    {
        return Shell.Current.GoToAsync(Routes.CloudSync);
    }

    [RelayCommand]
    private Task OpenPasswordPoliciesAsync()
    {
        return Shell.Current.GoToAsync(Routes.PasswordPolicies);
    }

    [RelayCommand]
    private Task OpenAboutAsync()
    {
        return Shell.Current.GoToAsync(Routes.About);
    }

    [RelayCommand]
    private Task OpenPrivacyPolicyAsync()
    {
        return Shell.Current.GoToAsync(Routes.PrivacyPolicy);
    }

    [RelayCommand]
    private Task OpenTermsOfServiceAsync()
    {
        return Shell.Current.GoToAsync(Routes.TermsOfService);
    }

    [RelayCommand]
    private async Task EditLanguageAsync()
    {
        if (Shell.Current == null)
        {
            return;
        }

        var result = await Shell.Current.DisplayPromptAsync(
            "Language",
            "Enter a language code (e.g. en)",
            accept: "OK",
            cancel: "Cancel",
            placeholder: "en",
            initialValue: LanguageDisplayName);

        if (result == null)
        {
            return;
        }

        Language = string.IsNullOrWhiteSpace(result) ? "en" : result.Trim();
    }

    [RelayCommand]
    private Task EditAutoLockAsync() => EditNonNegativeIntAsync(
        title: "Auto-lock Timeout",
        message: "Minutes",
        currentValue: AutoLockMinutes,
        onSet: v => AutoLockMinutes = v);

    [RelayCommand]
    private Task EditClipboardClearAsync() => EditNonNegativeIntAsync(
        title: "Clear Clipboard",
        message: "Seconds",
        currentValue: ClipboardClearSeconds,
        onSet: v => ClipboardClearSeconds = v);

    [RelayCommand]
    private Task EditMaxBackupCountAsync() => EditNonNegativeIntAsync(
        title: "Max Backup Count",
        message: "Count",
        currentValue: MaxBackupCount,
        onSet: v => MaxBackupCount = v);

    [RelayCommand]
    private async Task RenameDatabaseAsync()
    {
        if (Shell.Current == null)
        {
            return;
        }

        if (!_vaultSession.IsUnlocked)
        {
            ErrorMessage = "Unlock the vault first.";
            return;
        }

        var currentPath = _vaultSession.CurrentFilePath;
        if (string.IsNullOrWhiteSpace(currentPath))
        {
            ErrorMessage = "Current vault file not found.";
            return;
        }

        try
        {
            var config = await _store.LoadAsync();
            var currentAlias = FindAliasForPath(config, currentPath);
            var currentName = string.IsNullOrWhiteSpace(currentAlias)
                ? Path.GetFileNameWithoutExtension(currentPath)
                : currentAlias;

            var result = await Shell.Current.DisplayPromptAsync(
                "Rename Database",
                "Enter a new name",
                accept: "Rename",
                cancel: "Cancel",
                placeholder: currentName,
                initialValue: currentName);

            if (result == null)
            {
                return;
            }

            var trimmed = result.Trim();
            if (string.IsNullOrWhiteSpace(trimmed))
            {
                ErrorMessage = "Database name cannot be empty.";
                return;
            }

            if (!string.Equals(trimmed, currentAlias, StringComparison.OrdinalIgnoreCase)
                && config.Databases.ContainsKey(trimmed))
            {
                ErrorMessage = "A database with that name already exists.";
                return;
            }

            if (!string.IsNullOrWhiteSpace(currentAlias))
            {
                config.Databases.Remove(currentAlias);
            }

            config.Databases[trimmed] = currentPath;

            if (!string.IsNullOrWhiteSpace(config.DefaultDatabase)
                && string.Equals(config.DefaultDatabase, currentAlias, StringComparison.OrdinalIgnoreCase))
            {
                config.DefaultDatabase = trimmed;
            }

            await _store.SaveAsync(config);

            ActiveDatabaseName = trimmed;
            ErrorMessage = null;
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private async Task EditNonNegativeIntAsync(string title, string message, string currentValue, Action<string> onSet)
    {
        if (Shell.Current == null)
        {
            return;
        }

        var initial = int.TryParse(currentValue, out var parsed) && parsed >= 0 ? parsed.ToString() : "0";
        var result = await Shell.Current.DisplayPromptAsync(
            title,
            message,
            accept: "OK",
            cancel: "Cancel",
            placeholder: initial,
            initialValue: initial,
            keyboard: Keyboard.Numeric);

        if (result == null)
        {
            return;
        }

        if (!int.TryParse(result, out var value) || value < 0)
        {
            ErrorMessage = $"{title} must be a number ≥ 0.";
            return;
        }

        ErrorMessage = null;
        onSet(value.ToString());
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (!int.TryParse(AutoLockMinutes, out var autoLockMinutes) || autoLockMinutes < 0)
        {
            ErrorMessage = "Auto-lock minutes must be a number ≥ 0.";
            return;
        }

        if (!int.TryParse(ClipboardClearSeconds, out var clipboardClearSeconds) || clipboardClearSeconds < 0)
        {
            ErrorMessage = "Clipboard clear seconds must be a number ≥ 0.";
            return;
        }

        if (!int.TryParse(MaxBackupCount, out var maxBackupCount) || maxBackupCount < 0)
        {
            ErrorMessage = "Max backup count must be a number ≥ 0.";
            return;
        }

        var lang = (Language ?? string.Empty).Trim();
        if (string.IsNullOrWhiteSpace(lang))
        {
            lang = "en";
        }

        IsBusy = true;

        try
        {
            var config = await _store.LoadAsync();

            var wasBiometricEnabled = config.IsBiometricUnlockEnabled;

            config.Theme = Theme;
            config.IsBiometricUnlockEnabled = IsBiometricUnlockEnabled;
            var alias = string.IsNullOrWhiteSpace(_vaultSession.CurrentFilePath)
                ? null
                : FindAliasForPath(config, _vaultSession.CurrentFilePath);

            config.Language = lang;
            config.AutoLockMinutes = autoLockMinutes;
            config.ClipboardClearSeconds = clipboardClearSeconds;
            config.MaxBackupCount = maxBackupCount;

            if (IsDatabaseOpen && !string.IsNullOrWhiteSpace(alias))
            {
                config.DatabaseSyncStates[alias] = IsDatabaseSyncEnabled;
            }

            if (wasBiometricEnabled && !IsBiometricUnlockEnabled)
            {
                foreach (var filePath in config.Databases.Values)
                {
                    if (string.IsNullOrWhiteSpace(filePath))
                    {
                        continue;
                    }

                    try
                    {
                        var key = BiometricSecretKeys.ForVaultFilePath(filePath);
                        await _secretStore.RemoveAsync(key);
                    }
                    catch
                    {
                        // ignore; best-effort cleanup
                    }
                }
            }

            await _store.SaveAsync(config);

            ApplyThemeOverride();

            UpdateDatabaseState(config);
            await Shell.Current.GoToAsync("..");
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

    private void ApplyThemeOverride()
    {
        if (Application.Current == null)
        {
            return;
        }

        Application.Current.UserAppTheme = Theme switch
        {
            AppThemePreference.System => AppTheme.Unspecified,
            AppThemePreference.Dark => AppTheme.Dark,
            AppThemePreference.Light => AppTheme.Light,
            _ => AppTheme.Unspecified
        };
    }

    private void UpdateDatabaseState(AppConfiguration config)
    {
        IsDatabaseOpen = _vaultSession.IsUnlocked;

        var filePath = _vaultSession.CurrentFilePath;
        var alias = string.IsNullOrWhiteSpace(filePath) ? null : FindAliasForPath(config, filePath);
        ActiveDatabaseName = IsDatabaseOpen
            ? (alias ?? (string.IsNullOrWhiteSpace(filePath) ? "—" : Path.GetFileNameWithoutExtension(filePath)))
            : "—";

        StorageUsageDisplayName = BuildStorageUsageDisplayName(IsDatabaseOpen ? filePath : null, config);

        if (IsDatabaseOpen)
        {
            if (!string.IsNullOrWhiteSpace(alias))
            {
                IsDatabaseSyncEnabled = !config.DatabaseSyncStates.TryGetValue(alias, out var enabled) || enabled;
            }
            else
            {
                IsDatabaseSyncEnabled = true;
            }
        }
    }

    private static string? FindAliasForPath(AppConfiguration config, string filePath)
    {
        foreach (var pair in config.Databases)
        {
            if (string.Equals(pair.Value, filePath, StringComparison.OrdinalIgnoreCase))
            {
                return pair.Key;
            }
        }

        return null;
    }

    private static string BuildStorageUsageDisplayName(string? currentPath, AppConfiguration config)
    {
        long totalBytes = 0;

        if (!string.IsNullOrWhiteSpace(currentPath))
        {
            try
            {
                if (File.Exists(currentPath))
                {
                    totalBytes = new FileInfo(currentPath).Length;
                }
            }
            catch
            {
                return "—";
            }
        }
        else
        {
            foreach (var path in config.Databases.Values)
            {
                if (string.IsNullOrWhiteSpace(path))
                {
                    continue;
                }

                try
                {
                    if (!File.Exists(path))
                    {
                        continue;
                    }

                    totalBytes += new FileInfo(path).Length;
                }
                catch
                {
                    // ignore missing/unavailable file
                }
            }
        }

        if (totalBytes <= 0)
        {
            return "—";
        }

        var mb = totalBytes / (1024d * 1024d);
        return $"{mb:0.#} MB";
    }

    private static string BuildAppVersionDisplayName()
    {
        try
        {
            return $"Password Safe v{AppInfo.VersionString} (Build {AppInfo.BuildString})";
        }
        catch
        {
            return "Password Safe";
        }
    }
}
