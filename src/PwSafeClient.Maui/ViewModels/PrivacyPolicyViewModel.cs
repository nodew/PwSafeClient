using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class PrivacyPolicyViewModel : ObservableObject
{
    [RelayCommand]
    private Task CloseAsync() => Shell.Current.GoToAsync("..");
}
