using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages.DatabaseManagement;

public partial class ChangeMasterPasswordPage : ContentPage
{
    private readonly AutoLockService _autoLock;

    public ChangeMasterPasswordPage(ChangeMasterPasswordViewModel viewModel, AutoLockService autoLock)
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
