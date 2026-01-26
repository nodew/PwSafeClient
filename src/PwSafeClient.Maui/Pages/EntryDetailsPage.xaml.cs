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

        Shell.SetBackButtonBehavior(this, new BackButtonBehavior
        {
            Command = new Command(async () => await NavigateBackToVault())
        });
    }

    protected override bool OnBackButtonPressed()
    {
        _ = NavigateBackToVault();
        return true;
    }

    private async Task NavigateBackToVault()
    {
        var nav = Shell.Current.Navigation;
        var stack = nav.NavigationStack;

        var vaultPage = stack.FirstOrDefault(p => p is VaultPage);
        if (vaultPage != null)
        {
            var vaultIndex = -1;
            for (int i = 0; i < stack.Count; i++)
            {
                if (stack[i] == vaultPage)
                {
                    vaultIndex = i;
                    break;
                }
            }

            if (vaultIndex != -1)
            {
                var pagesToRemove = new List<Page>();
                for (int i = vaultIndex + 1; i < stack.Count - 1; i++)
                {
                    pagesToRemove.Add(stack[i]);
                }

                foreach (var page in pagesToRemove)
                {
                    nav.RemovePage(page);
                }
            }
        }

        await Shell.Current.GoToAsync("..");
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
