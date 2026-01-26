using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages;

public partial class SettingsPage : ContentPage
{
    private readonly SettingsViewModel _viewModel;
    private readonly AutoLockService _autoLock;

    public SettingsPage(SettingsViewModel viewModel, AutoLockService autoLock)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _autoLock = autoLock;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _autoLock.NotifyActivity();
        _viewModel.LoadCommand.Execute(null);
    }
}
