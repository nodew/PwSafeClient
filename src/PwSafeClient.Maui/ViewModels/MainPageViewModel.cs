using CommunityToolkit.Mvvm.ComponentModel;

namespace PwSafeClient.Maui.ViewModels;

public partial class MainPageViewModel : ObservableObject
{
    [ObservableProperty]
    private string dbFilePath = string.Empty;

    [ObservableProperty]
    private string masterPassword = string.Empty;

    [ObservableProperty]
    private bool isActivatedFromFile = false;
}
