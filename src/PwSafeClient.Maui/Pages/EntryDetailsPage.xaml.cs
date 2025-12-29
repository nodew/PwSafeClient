using System.Collections.Generic;

using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages;

public partial class EntryDetailsPage : ContentPage, IQueryAttributable
{
    private readonly EntryDetailsViewModel _viewModel;
    private readonly AutoLockService _autoLock;

    public EntryDetailsPage(EntryDetailsViewModel viewModel, AutoLockService autoLock)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _autoLock = autoLock;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("index", out var indexObj) && indexObj is string indexText && int.TryParse(indexText, out var index))
        {
            _viewModel.SetEntryIndex(index);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _autoLock.NotifyActivity();
        _viewModel.Refresh();
    }
}
