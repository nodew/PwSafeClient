using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class PasswordPoliciesViewModel : ObservableObject
{
    private bool _isSheetVisible;
    public bool IsSheetVisible
    {
        get => _isSheetVisible;
        set => SetProperty(ref _isSheetVisible, value);
    }

    private string _sheetTitle = "Edit Policy";
    public string SheetTitle
    {
        get => _sheetTitle;
        set => SetProperty(ref _sheetTitle, value);
    }

    private string _policyName = string.Empty;
    public string PolicyName
    {
        get => _policyName;
        set => SetProperty(ref _policyName, value);
    }

    private string _passwordLength = "16";
    public string PasswordLength
    {
        get => _passwordLength;
        set => SetProperty(ref _passwordLength, value);
    }

    private string _uppercaseCount = "2";
    public string UppercaseCount
    {
        get => _uppercaseCount;
        set => SetProperty(ref _uppercaseCount, value);
    }

    private string _lowercaseCount = "6";
    public string LowercaseCount
    {
        get => _lowercaseCount;
        set => SetProperty(ref _lowercaseCount, value);
    }

    private string _numberCount = "4";
    public string NumberCount
    {
        get => _numberCount;
        set => SetProperty(ref _numberCount, value);
    }

    private string _symbolCount = "2";
    public string SymbolCount
    {
        get => _symbolCount;
        set => SetProperty(ref _symbolCount, value);
    }

    private string _customSymbols = string.Empty;
    public string CustomSymbols
    {
        get => _customSymbols;
        set => SetProperty(ref _customSymbols, value);
    }

    private bool _canDeletePolicy;
    public bool CanDeletePolicy
    {
        get => _canDeletePolicy;
        set => SetProperty(ref _canDeletePolicy, value);
    }

    [RelayCommand]
    private Task CloseAsync() => Shell.Current.GoToAsync("..");

    [RelayCommand]
    private void OpenEditPolicy(string? name)
    {
        SheetTitle = "Edit Policy";
        PolicyName = name ?? "Standard";
        PasswordLength = name == "PIN" ? "6" : "16";
        UppercaseCount = name == "PIN" ? "0" : "2";
        LowercaseCount = name == "PIN" ? "0" : "6";
        NumberCount = name == "PIN" ? "6" : "4";
        SymbolCount = name == "Strong" ? "2" : "0";
        CustomSymbols = name == "Strong" ? "!@#$%" : string.Empty;
        CanDeletePolicy = true;
        IsSheetVisible = true;
    }

    [RelayCommand]
    private void OpenCreatePolicy()
    {
        SheetTitle = "Create Policy";
        PolicyName = string.Empty;
        PasswordLength = "16";
        UppercaseCount = "2";
        LowercaseCount = "6";
        NumberCount = "4";
        SymbolCount = "2";
        CustomSymbols = string.Empty;
        CanDeletePolicy = false;
        IsSheetVisible = true;
    }

    [RelayCommand]
    private void CloseSheet()
    {
        IsSheetVisible = false;
    }

    [RelayCommand]
    private Task SaveSheetAsync()
    {
        IsSheetVisible = false;

        var shell = Shell.Current;
        return shell?.DisplayAlertAsync("Password Policies", "Policy saved (preview only).", "OK") ?? Task.CompletedTask;
    }

    [RelayCommand]
    private Task SaveAsDefaultAsync()
    {
        IsSheetVisible = false;

        var shell = Shell.Current;
        return shell?.DisplayAlertAsync("Password Policies", "Default policy updated (preview only).", "OK") ?? Task.CompletedTask;
    }

    [RelayCommand]
    private Task DeletePolicyAsync()
    {
        IsSheetVisible = false;

        var shell = Shell.Current;
        return shell?.DisplayAlertAsync("Password Policies", "Policy deleted (preview only).", "OK") ?? Task.CompletedTask;
    }
}
