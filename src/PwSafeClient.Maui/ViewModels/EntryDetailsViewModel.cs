using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.CloudSync;
using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Vault;
using PwSafeClient.AppCore.Vault.Editing;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class EntryDetailsViewModel : ObservableObject
{
    private readonly IVaultSession _vaultSession;
    private readonly IAppConfigurationStore _configStore;
    private readonly ICloudSyncService _cloudSyncService;

    private CancellationTokenSource? _clipboardCts;

    public EntryDetailsViewModel(IVaultSession vaultSession, IAppConfigurationStore configStore, ICloudSyncService cloudSyncService)
    {
        _vaultSession = vaultSession;
        _configStore = configStore;
        _cloudSyncService = cloudSyncService;
    }

    private int? _entryIndex;

    public void SetEntryIndex(int index)
    {
        _entryIndex = index;
        Load();
    }

    public void Refresh()
    {
        if (_entryIndex is null)
        {
            return;
        }

        Load();
    }

    private string? _title;
    public string? Title
    {
        get => _title;
        private set => SetProperty(ref _title, value);
    }

    private string? _username;
    public string? Username
    {
        get => _username;
        private set => SetProperty(ref _username, value);
    }

    private string? _url;
    public string? Url
    {
        get => _url;
        private set => SetProperty(ref _url, value);
    }

    private string? _notes;
    public string? Notes
    {
        get => _notes;
        private set => SetProperty(ref _notes, value);
    }

    private bool _isPasswordRevealed;
    public bool IsPasswordRevealed
    {
        get => _isPasswordRevealed;
        private set
        {
            if (SetProperty(ref _isPasswordRevealed, value))
            {
                OnPropertyChanged(nameof(TogglePasswordButtonText));
            }
        }
    }

    public string TogglePasswordButtonText => IsPasswordRevealed ? "Hide" : "Show";

    private string? _passwordDisplay;
    public string? PasswordDisplay
    {
        get => _passwordDisplay;
        private set => SetProperty(ref _passwordDisplay, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        private set => SetProperty(ref _errorMessage, value);
    }

    private void Load()
    {
        ErrorMessage = null;
        Title = null;
        Username = null;
        Url = null;
        Notes = null;
        IsPasswordRevealed = false;
        PasswordDisplay = "••••••••";

        if (_entryIndex is null)
        {
            ErrorMessage = "Entry not specified.";
            return;
        }

        var details = _vaultSession.GetEntryDetailsSnapshot(_entryIndex.Value, includePassword: false);
        if (details == null)
        {
            ErrorMessage = "Entry not found.";
            return;
        }

        Title = details.Title;
        Username = details.UserName;
        Url = details.Url;
        Notes = details.Notes;
    }

    [RelayCommand]
    private Task EditAsync()
    {
        if (_entryIndex is null)
        {
            return Task.CompletedTask;
        }

        return Shell.Current.GoToAsync($"{Routes.EntryEdit}?index={_entryIndex.Value}");
    }

    [RelayCommand]
    private async Task DeleteAsync()
    {
        ErrorMessage = null;

        if (_entryIndex is null)
        {
            return;
        }

        var confirm = await Shell.Current.DisplayAlertAsync(
            "Delete entry",
            "Are you sure you want to delete this entry?",
            "Delete",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        VaultEntryDeleteResult result;
        try
        {
            result = _vaultSession.DeleteEntry(_entryIndex.Value);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
            return;
        }

        if (!result.IsSuccess)
        {
            ErrorMessage = result.ErrorMessage ?? "Failed to delete entry.";
            return;
        }

        try
        {
            await _vaultSession.SaveAsync();
            await TriggerCloudSyncIfEnabledAsync();
            await Shell.Current.GoToAsync("..");
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private Task TriggerCloudSyncIfEnabledAsync()
        => _cloudSyncService.TriggerSyncIfEnabledAsync(CloudSyncTrigger.Save);

    [RelayCommand]
    private Task TogglePasswordAsync()
    {
        if (_entryIndex is null)
        {
            return Task.CompletedTask;
        }

        if (!IsPasswordRevealed)
        {
            var details = _vaultSession.GetEntryDetailsSnapshot(_entryIndex.Value, includePassword: true);
            PasswordDisplay = string.IsNullOrEmpty(details?.Password) ? "(empty)" : details!.Password;
            IsPasswordRevealed = true;
        }
        else
        {
            PasswordDisplay = "••••••••";
            IsPasswordRevealed = false;
        }

        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task CopyUsernameAsync()
    {
        if (string.IsNullOrWhiteSpace(Username))
        {
            return;
        }

        await CopyToClipboardAndScheduleClearAsync(Username);
    }

    [RelayCommand]
    private async Task CopyPasswordAsync()
    {
        if (_entryIndex is null)
        {
            return;
        }

        var details = _vaultSession.GetEntryDetailsSnapshot(_entryIndex.Value, includePassword: true);
        if (string.IsNullOrEmpty(details?.Password))
        {
            return;
        }

        await CopyToClipboardAndScheduleClearAsync(details!.Password);
    }

    private async Task CopyToClipboardAndScheduleClearAsync(string text)
    {
        await Clipboard.Default.SetTextAsync(text);

        _clipboardCts?.Cancel();
        _clipboardCts?.Dispose();
        _clipboardCts = new CancellationTokenSource();

        var seconds = 30;
        try
        {
            var cfg = await _configStore.LoadAsync(_clipboardCts.Token);
            if (cfg.ClipboardClearSeconds >= 0)
            {
                seconds = cfg.ClipboardClearSeconds;
            }
        }
        catch
        {
            // ignore config read failures; fallback is fine
        }

        if (seconds <= 0)
        {
            return;
        }

        _ = ClearClipboardLaterAsync(TimeSpan.FromSeconds(seconds), _clipboardCts.Token);
    }

    private static async Task ClearClipboardLaterAsync(TimeSpan delay, CancellationToken cancellationToken)
    {
        try
        {
            await Task.Delay(delay, cancellationToken);
            await Clipboard.Default.SetTextAsync(string.Empty);
        }
        catch (OperationCanceledException)
        {
            // expected
        }
        catch
        {
            // ignore clipboard failures
        }
    }
}
