using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages.Settings;

public partial class PrivacyPolicyPage : ContentPage
{
    private readonly AutoLockService _autoLock;

    public PrivacyPolicyPage(PrivacyPolicyViewModel viewModel, AutoLockService autoLock)
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
