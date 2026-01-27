using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace PwSafeClient.Maui.ViewModels;

public sealed partial class TermsOfServiceViewModel : ObservableObject
{
    [RelayCommand]
    private Task CloseAsync() => Shell.Current.GoToAsync("..");
}
