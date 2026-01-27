using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class CloudSyncViewModel : ObservableObject
{
    private string _syncStatus = "Not configured";
    public string SyncStatus
    {
        get => _syncStatus;
        set => SetProperty(ref _syncStatus, value);
    }

    private string _lastSyncDisplayName = "—";
    public string LastSyncDisplayName
    {
        get => _lastSyncDisplayName;
        set => SetProperty(ref _lastSyncDisplayName, value);
    }

    private string _providerDisplayName = "Not configured";
    public string ProviderDisplayName
    {
        get => _providerDisplayName;
        set => SetProperty(ref _providerDisplayName, value);
    }

    private string _accountDisplayName = "—";
    public string AccountDisplayName
    {
        get => _accountDisplayName;
        set => SetProperty(ref _accountDisplayName, value);
    }

    private bool _syncOnSave;
    public bool SyncOnSave
    {
        get => _syncOnSave;
        set => SetProperty(ref _syncOnSave, value);
    }

    private string _syncFrequencyDisplayName = "—";
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

    public string PlaceholderMessage => "Cloud sync providers are not configured yet.";

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
