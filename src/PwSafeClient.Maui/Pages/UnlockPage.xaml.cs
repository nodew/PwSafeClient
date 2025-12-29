using System.Collections.Generic;

using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages;

public partial class UnlockPage : ContentPage, IQueryAttributable
{
    private readonly UnlockViewModel _viewModel;
    private readonly AutoLockService _autoLock;

    public UnlockPage(UnlockViewModel viewModel, AutoLockService autoLock)
    {
        InitializeComponent();
        _viewModel = viewModel;
        _autoLock = autoLock;
        BindingContext = viewModel;
    }

    public void ApplyQueryAttributes(IDictionary<string, object> query)
    {
        if (query.TryGetValue("alias", out var aliasObj) && aliasObj is string alias && !string.IsNullOrWhiteSpace(alias))
        {
            MainThread.BeginInvokeOnMainThread(async () => await _viewModel.SetAliasAsync(alias));
            return;
        }

        query.TryGetValue("filePath", out var filePathObj);
        var filePath = filePathObj as string;
        _viewModel.SetFilePath(filePath);
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        _autoLock.NotifyActivity();
    }
}
