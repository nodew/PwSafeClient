using CommunityToolkit.Mvvm.ComponentModel;

namespace PwSafeClient.Maui.ViewModels;

public partial class AppShellViewModel : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(HideManagerView))]
    private bool showManagerView = false;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDatabaseClosed))]
    private bool isDatabaseOpen = false;

    public bool HideManagerView => !ShowManagerView;

    public bool IsDatabaseClosed => !IsDatabaseOpen;
}
