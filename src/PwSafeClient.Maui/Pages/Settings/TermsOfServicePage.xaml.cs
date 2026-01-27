using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages.Settings;

public partial class TermsOfServicePage : ContentPage
{
    private readonly AutoLockService _autoLock;

    public TermsOfServicePage(TermsOfServiceViewModel viewModel, AutoLockService autoLock)
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
