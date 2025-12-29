using System.Collections.Generic;

using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages;

public partial class EntryEditPage : ContentPage, IQueryAttributable
{
    private readonly EntryEditViewModel _viewModel;
    private readonly AutoLockService _autoLock;

    public EntryEditPage(EntryEditViewModel viewModel, AutoLockService autoLock)
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
            _viewModel.SetEditIndex(index);
        }

        if (query.TryGetValue("group", out var groupObj) && groupObj is string groupText)
        {
            _viewModel.SetDefaultGroup(groupText);
        }
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _autoLock.NotifyActivity();
    }
}
