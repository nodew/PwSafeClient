using System;
using System.Threading.Tasks;

using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Security.Secrets;
using PwSafeClient.Maui.Services.Security;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class SettingsViewModel : ObservableObject
{
    private readonly IAppConfigurationStore _store;
    private readonly ISecureSecretStore _secretStore;

    public SettingsViewModel(IAppConfigurationStore store, ISecureSecretStore secretStore)
    {
        _store = store;
        _secretStore = secretStore;
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
    private Task CloseAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

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
            config.Language = lang;
            config.AutoLockMinutes = autoLockMinutes;
            config.ClipboardClearSeconds = clipboardClearSeconds;
            config.MaxBackupCount = maxBackupCount;

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
}
