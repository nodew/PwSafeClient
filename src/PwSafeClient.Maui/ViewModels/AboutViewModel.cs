using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Maui.ApplicationModel;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class AboutViewModel : ObservableObject
{
    private string _appVersionDisplayName = AppInfo.VersionString;
    public string AppVersionDisplayName
    {
        get => _appVersionDisplayName;
        set => SetProperty(ref _appVersionDisplayName, value);
    }

    private string _buildDisplayName = AppInfo.BuildString;
    public string BuildDisplayName
    {
        get => _buildDisplayName;
        set => SetProperty(ref _buildDisplayName, value);
    }

    private string? _errorMessage;
    public string? ErrorMessage
    {
        get => _errorMessage;
        set => SetProperty(ref _errorMessage, value);
    }

    [RelayCommand]
    private Task CloseAsync() => Shell.Current.GoToAsync("..");
}
