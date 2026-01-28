using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages.Settings;

public partial class CloudSyncPage : ContentPage
{
    private readonly AutoLockService _autoLock;
    private readonly CloudSyncViewModel _viewModel;

    public CloudSyncPage(CloudSyncViewModel viewModel, AutoLockService autoLock)
    {
        InitializeComponent();
        _viewModel = viewModel;
        BindingContext = viewModel;
        _autoLock = autoLock;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _autoLock.NotifyActivity();
        _viewModel.LoadCommand.Execute(null);
    }
}
