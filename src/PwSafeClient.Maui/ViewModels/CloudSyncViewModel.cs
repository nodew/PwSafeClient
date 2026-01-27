using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class CloudSyncViewModel : ObservableObject
{
    private string _syncStatus = "Connected";
    public string SyncStatus
    {
        get => _syncStatus;
        set => SetProperty(ref _syncStatus, value);
    }

    private string _lastSyncDisplayName = "10 minutes ago";
    public string LastSyncDisplayName
    {
        get => _lastSyncDisplayName;
        set => SetProperty(ref _lastSyncDisplayName, value);
    }

    private string _providerDisplayName = "Dropbox";
    public string ProviderDisplayName
    {
        get => _providerDisplayName;
        set => SetProperty(ref _providerDisplayName, value);
    }

    private string _accountDisplayName = "user@example.com";
    public string AccountDisplayName
    {
        get => _accountDisplayName;
        set => SetProperty(ref _accountDisplayName, value);
    }

    private bool _syncOnSave = true;
    public bool SyncOnSave
    {
        get => _syncOnSave;
        set => SetProperty(ref _syncOnSave, value);
    }

    private string _syncFrequencyDisplayName = "Every 30 minutes";
    public string SyncFrequencyDisplayName
    {
        get => _syncFrequencyDisplayName;
        set => SetProperty(ref _syncFrequencyDisplayName, value);
    }

    private bool _syncOnCellular;
    public bool SyncOnCellular
    {
        get => _syncOnCellular;
        set => SetProperty(ref _syncOnCellular, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    [RelayCommand]
    private Task CloseAsync() => Shell.Current.GoToAsync("..");

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

        SyncStatus = "Disconnected";
        AccountDisplayName = "—";
    }
}
