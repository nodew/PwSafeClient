using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages.DatabaseManagement;

public partial class CreateDatabasePage : ContentPage
{
    private readonly AutoLockService _autoLock;

    public CreateDatabasePage(CreateDatabaseViewModel viewModel, AutoLockService autoLock)
    {
        InitializeComponent();
        _autoLock = autoLock;
        BindingContext = viewModel;
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _autoLock.NotifyActivity();
    }
}
