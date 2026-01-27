using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages.Settings;

public partial class CloudSyncPage : ContentPage
{
    private readonly AutoLockService _autoLock;

    public CloudSyncPage(CloudSyncViewModel viewModel, AutoLockService autoLock)
    {
        InitializeComponent();
        BindingContext = viewModel;
        _autoLock = autoLock;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _autoLock.NotifyActivity();
    }
}
