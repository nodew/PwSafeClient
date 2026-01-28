using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.CloudSync;
using PwSafeClient.AppCore.Configuration;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class CloudSyncViewModel : ObservableObject
{
    private readonly ICloudSyncService _cloudSyncService;
    private CloudSyncState? _state;
    private bool _isInitializing;
    private bool _suppressOptionUpdates;

    public CloudSyncViewModel(ICloudSyncService cloudSyncService)
    {
        _cloudSyncService = cloudSyncService;
    }

    private string _syncStatus = "Not configured";
    public string SyncStatus
    {
        get => _syncStatus;
        private set => SetProperty(ref _syncStatus, value);
    }

    private string _lastSyncDisplayName = "—";
    public string LastSyncDisplayName
    {
        get => _lastSyncDisplayName;
        private set => SetProperty(ref _lastSyncDisplayName, value);
    }

    private string _providerDisplayName = "Not configured";
    public string ProviderDisplayName
    {
        get => _providerDisplayName;
        private set => SetProperty(ref _providerDisplayName, value);
    }

    private string _accountDisplayName = "—";
    public string AccountDisplayName
    {
        get => _accountDisplayName;
        private set => SetProperty(ref _accountDisplayName, value);
    }

    private bool _syncOnSave;
    public bool SyncOnSave
    {
        get => _syncOnSave;
        set
        {
            if (SetProperty(ref _syncOnSave, value) && !_suppressOptionUpdates)
            {
                SaveOptionsCommand.Execute(null);
            }
        }
    }

    private string _syncFrequencyDisplayName = "—";
    public string SyncFrequencyDisplayName
    {
        get => _syncFrequencyDisplayName;
        private set => SetProperty(ref _syncFrequencyDisplayName, value);
    }

    private bool _syncOnCellular;
    public bool SyncOnCellular
    {
        get => _syncOnCellular;
        set
        {
            if (SetProperty(ref _syncOnCellular, value) && !_suppressOptionUpdates)
            {
                SaveOptionsCommand.Execute(null);
            }
        }
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

    public string PlaceholderMessage => "Pick a provider and schedule to enable cloud sync.";

    [RelayCommand]
    public async Task LoadAsync()
    {
        ErrorMessage = null;
        IsBusy = true;
        _isInitializing = true;

        try
        {
            _state = await _cloudSyncService.GetStateAsync();
            UpdateFromState(_state);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            _isInitializing = false;
            IsBusy = false;
        }
    }

    [RelayCommand]
    private Task CloseAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private async Task SelectProviderAsync()
    {
        if (Shell.Current == null)
        {
            return;
        }

        var choice = await Shell.Current.DisplayActionSheetAsync(
            "Cloud Provider",
            "Cancel",
            null,
            "iCloud Drive",
            "Google Drive",
            "Dropbox",
            "OneDrive");

        if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
        {
            return;
        }

        var provider = choice switch
        {
            "iCloud Drive" => CloudSyncProvider.ICloudDrive,
            "Google Drive" => CloudSyncProvider.GoogleDrive,
            "Dropbox" => CloudSyncProvider.Dropbox,
            "OneDrive" => CloudSyncProvider.OneDrive,
            _ => CloudSyncProvider.None
        };

        if (provider == CloudSyncProvider.None)
        {
            return;
        }

        var account = await Shell.Current.DisplayPromptAsync(
            "Account",
            "Enter account name",
            accept: "Save",
            cancel: "Cancel",
            placeholder: "name@example.com",
            initialValue: AccountDisplayName == "—" ? string.Empty : AccountDisplayName);

        if (account == null)
        {
            return;
        }

        var state = await _cloudSyncService.GetStateAsync();
        var updated = state with
        {
            Provider = provider,
            AccountName = string.IsNullOrWhiteSpace(account) ? state.AccountName : account.Trim()
        };

        await _cloudSyncService.UpdateOptionsAsync(updated);
        _state = await _cloudSyncService.GetStateAsync();
        UpdateFromState(_state);
    }

    [RelayCommand]
    private async Task SelectFrequencyAsync()
    {
        if (Shell.Current == null)
        {
            return;
        }

        var choice = await Shell.Current.DisplayActionSheetAsync(
            "Sync Frequency",
            "Cancel",
            null,
            "Manual",
            "Hourly",
            "Daily",
            "Weekly");

        if (string.IsNullOrWhiteSpace(choice) || choice == "Cancel")
        {
            return;
        }

        var schedule = choice switch
        {
            "Manual" => CloudSyncSchedule.Manual,
            "Hourly" => CloudSyncSchedule.Hourly,
            "Daily" => CloudSyncSchedule.Daily,
            "Weekly" => CloudSyncSchedule.Weekly,
            _ => CloudSyncSchedule.Daily
        };

        var state = await _cloudSyncService.GetStateAsync();
        await _cloudSyncService.UpdateOptionsAsync(state with { Schedule = schedule });
        _state = await _cloudSyncService.GetStateAsync();
        UpdateFromState(_state);
    }

    [RelayCommand]
    private async Task SyncNowAsync()
    {
        ErrorMessage = null;
        IsBusy = true;

        try
        {
            var result = await _cloudSyncService.TriggerSyncAsync(CloudSyncTrigger.Manual);
            if (!result.IsSuccess)
            {
                ErrorMessage = result.ErrorMessage ?? "Sync failed.";
            }
            else if (result.HadConflict && Shell.Current != null)
            {
                var resolve = await Shell.Current.DisplayAlertAsync(
                    "Sync Conflict",
                    "A newer cloud copy was found. Override it with the local vault?",
                    "Overwrite",
                    "Cancel");

                if (resolve)
                {
                    var retry = await _cloudSyncService.TriggerSyncAsync(CloudSyncTrigger.ConflictResolution);
                    if (!retry.IsSuccess)
                    {
                        ErrorMessage = retry.ErrorMessage ?? "Conflict resolution failed.";
                    }
                }
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            _state = await _cloudSyncService.GetStateAsync();
            UpdateFromState(_state);
            IsBusy = false;
        }
    }

    [RelayCommand]
    private async Task DisconnectAsync()
    {
        ErrorMessage = null;

        var shell = Shell.Current;
        if (shell == null)
        {
            return;
        }

        var confirm = await shell.DisplayAlertAsync(
            "Disconnect",
            "Disconnect the current sync provider?",
            "Disconnect",
            "Cancel");

        if (!confirm)
        {
            return;
        }

        await _cloudSyncService.DisconnectAsync();
        _state = await _cloudSyncService.GetStateAsync();
        UpdateFromState(_state);
    }

    private async Task SaveOptionsAsync()
    {
        if (_isInitializing)
        {
            return;
        }

        try
        {
            var state = _state ?? await _cloudSyncService.GetStateAsync();
            _state = state with
            {
                SyncOnSave = SyncOnSave,
                SyncOnCellular = SyncOnCellular
            };

            await _cloudSyncService.UpdateOptionsAsync(_state);
            _state = await _cloudSyncService.GetStateAsync();
            UpdateFromState(_state);
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
    }

    private void UpdateFromState(CloudSyncState? state)
    {
        if (state == null)
        {
            SyncStatus = "Not configured";
            ProviderDisplayName = "Not configured";
            AccountDisplayName = "—";
            LastSyncDisplayName = "—";
            SyncFrequencyDisplayName = "—";
            SyncOnSave = false;
            SyncOnCellular = false;
            return;
        }

        SyncStatus = state.Status switch
        {
            CloudSyncStatus.NotConfigured => "Not configured",
            CloudSyncStatus.Ready => "Ready",
            CloudSyncStatus.Syncing => "Syncing",
            CloudSyncStatus.SyncFailed => "Sync failed",
            CloudSyncStatus.Disconnected => "Disconnected",
            _ => "Not configured"
        };

        ProviderDisplayName = state.Provider switch
        {
            CloudSyncProvider.ICloudDrive => "iCloud Drive",
            CloudSyncProvider.GoogleDrive => "Google Drive",
            CloudSyncProvider.Dropbox => "Dropbox",
            CloudSyncProvider.OneDrive => "OneDrive",
            _ => "Not configured"
        };

        AccountDisplayName = string.IsNullOrWhiteSpace(state.AccountName) ? "—" : state.AccountName;
        LastSyncDisplayName = state.LastSyncedAt.HasValue
            ? state.LastSyncedAt.Value.ToLocalTime().ToString("g")
            : "—";

        SyncFrequencyDisplayName = state.Schedule switch
        {
            CloudSyncSchedule.Manual => "Manual",
            CloudSyncSchedule.Hourly => "Hourly",
            CloudSyncSchedule.Daily => "Daily",
            CloudSyncSchedule.Weekly => "Weekly",
            _ => "—"
        };

        _suppressOptionUpdates = true;
        SyncOnSave = state.SyncOnSave;
        SyncOnCellular = state.SyncOnCellular;
        _suppressOptionUpdates = false;
    }
}
