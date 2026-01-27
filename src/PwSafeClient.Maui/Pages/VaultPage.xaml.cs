using PwSafeClient.Maui.Models;
using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages;

public partial class VaultPage : ContentPage
{
    private readonly VaultViewModel _viewModel;
    private readonly AutoLockService _autoLock;

    public VaultPage(VaultViewModel viewModel, AutoLockService autoLock)
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
        _viewModel.Refresh();
    }

    private void OnDragStarting(object sender, DragStartingEventArgs e)
    {
        if (BindingContext is not VaultViewModel viewModel)
        {
            return;
        }

        var item = (sender as BindableObject)?.BindingContext as VaultListItem;
        if (viewModel.StartDragCommand.CanExecute(item))
        {
            viewModel.StartDragCommand.Execute(item);
        }
    }

    private void OnDropOnItem(object sender, DropEventArgs e)
    {
        if (BindingContext is not VaultViewModel viewModel)
        {
            return;
        }

        var item = (sender as BindableObject)?.BindingContext as VaultListItem;
        if (viewModel.DropOnItemCommand.CanExecute(item))
        {
            viewModel.DropOnItemCommand.Execute(item);
        }
    }
}
