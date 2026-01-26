using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages;

public partial class DatabaseListPage : ContentPage
{
    private readonly DatabaseListViewModel _viewModel;
    private readonly AutoLockService _autoLock;

    public DatabaseListPage(DatabaseListViewModel viewModel, AutoLockService autoLock)
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

    private async void OnSettingsClicked(object? sender, EventArgs e)
    {
        await Shell.Current.GoToAsync(Routes.Settings);
    }
}
