using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

using PwSafeClient.AppCore.Vault;
using PwSafeClient.AppCore.Vault.Editing;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class EntryEditViewModel : ObservableObject
{
    private readonly IVaultSession _vaultSession;

    private int? _editIndex;
    private string? _defaultGroup;

    public EntryEditViewModel(IVaultSession vaultSession)
    {
        _vaultSession = vaultSession;
    }

    public void SetEditIndex(int index)
    {
        _editIndex = index;
        LoadFromVault();
    }

    public void SetDefaultGroup(string? group)
    {
        _defaultGroup = group;
        if (_editIndex == null && string.IsNullOrWhiteSpace(GroupPath))
        {
            GroupPath = group;
        }
    }

    public string PageTitle => _editIndex.HasValue ? "Edit Entry" : "New Entry";

    private string _title = string.Empty;
    public string Title
    {
        get => _title;
        set => SetProperty(ref _title, value);
    }

    private string _username = string.Empty;
    public string Username
    {
        get => _username;
        set => SetProperty(ref _username, value);
    }

    private string _password = string.Empty;
    public string Password
    {
        get => _password;
        set => SetProperty(ref _password, value);
    }

    private bool _isPasswordHidden = true;
    public bool IsPasswordHidden
    {
        get => _isPasswordHidden;
        private set
        {
            if (SetProperty(ref _isPasswordHidden, value))
            {
                OnPropertyChanged(nameof(TogglePasswordButtonText));
            }
        }
    }

    public string TogglePasswordButtonText => IsPasswordHidden ? "Show Password" : "Hide Password";

    private string _url = string.Empty;
    public string Url
    {
        get => _url;
        set => SetProperty(ref _url, value);
    }

    private string _notes = string.Empty;
    public string Notes
    {
        get => _notes;
        set => SetProperty(ref _notes, value);
    }

    private string? _groupPath;
    public string? GroupPath
    {
        get => _groupPath;
        set => SetProperty(ref _groupPath, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    private bool _isBusy;
    public bool IsBusy
    {
        get => _isBusy;
        set => SetProperty(ref _isBusy, value);
    }

    [RelayCommand]
    private Task CancelAsync()
    {
        return Shell.Current.GoToAsync("..");
    }

    [RelayCommand]
    private Task TogglePasswordAsync()
    {
        IsPasswordHidden = !IsPasswordHidden;
        return Task.CompletedTask;
    }

    [RelayCommand]
    private async Task SaveAsync()
    {
        ErrorMessage = null;

        if (!_vaultSession.IsUnlocked)
        {
            ErrorMessage = "Vault is locked.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Title))
        {
            ErrorMessage = "Title is required.";
            return;
        }

        if (string.IsNullOrEmpty(Password))
        {
            ErrorMessage = "Password is required.";
            return;
        }

        IsBusy = true;

        try
        {
            var request = new VaultEntryEditRequest
            {
                Title = Title.Trim(),
                UserName = string.IsNullOrWhiteSpace(Username) ? null : Username,
                Password = Password,
                Url = string.IsNullOrWhiteSpace(Url) ? null : Url,
                Notes = string.IsNullOrWhiteSpace(Notes) ? null : Notes,
                GroupPath = string.IsNullOrWhiteSpace(GroupPath) ? _defaultGroup : GroupPath
            };

            VaultEntryUpsertResult result;
            if (_editIndex.HasValue)
            {
                result = _vaultSession.UpdateEntry(_editIndex.Value, request);
            }
            else
            {
                result = _vaultSession.CreateEntry(request);
            }

            if (!result.IsSuccess || result.EntryIndex is null)
            {
                ErrorMessage = result.ErrorMessage ?? "Failed to save entry.";
                return;
            }

            await _vaultSession.SaveAsync();

            // After create, go to details; after edit, go back.
            if (_editIndex.HasValue)
            {
                await Shell.Current.GoToAsync("..");
            }
            else
            {
                await Shell.Current.GoToAsync($"{Routes.EntryDetails}?index={result.EntryIndex.Value}");
            }
        }
        catch (Exception ex)
        {
            ErrorMessage = ex.Message;
        }
        finally
        {
            IsBusy = false;
        }
    }

    private void LoadFromVault()
    {
        OnPropertyChanged(nameof(PageTitle));

        if (!_editIndex.HasValue)
        {
            return;
        }

        var details = _vaultSession.GetEntryDetailsSnapshot(_editIndex.Value, includePassword: true);
        if (details == null)
        {
            ErrorMessage = "Entry not found.";
            return;
        }

        Title = details.Title;
        Username = details.UserName ?? string.Empty;
        Password = details.Password ?? string.Empty;
        Url = details.Url ?? string.Empty;
        Notes = details.Notes ?? string.Empty;
        GroupPath = details.GroupPath;
    }
}
