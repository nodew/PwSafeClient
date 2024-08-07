using Microsoft.Windows.AppLifecycle;

using PwSafeClient.Maui.ViewModels;

using Windows.ApplicationModel.Activation;

using LaunchActivatedEventArgs = Microsoft.UI.Xaml.LaunchActivatedEventArgs;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace PwSafeClient.Maui.WinUI
{
    /// <summary>
    /// Provides application-specific behavior to supplement the default Application class.
    /// </summary>
    public partial class App : MauiWinUIApplication
    {
        /// <summary>
        /// Initializes the singleton application object.  This is the first line of authored code
        /// executed, and as such is the logical equivalent of main() or WinMain().
        /// </summary>
        public App()
        {
            this.InitializeComponent();
        }

        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        protected override void OnLaunched(LaunchActivatedEventArgs args)
        {
            base.OnLaunched(args);

            var activatedArgs = AppInstance.GetCurrent().GetActivatedEventArgs();
            if (activatedArgs.Kind == ExtendedActivationKind.File)
            {
                var fileArgs = activatedArgs.Data as IFileActivatedEventArgs;
#pragma warning disable CA1826 // Do not use Enumerable methods on indexable collections
                var filePath = fileArgs?.Files.FirstOrDefault()?.Path;
#pragma warning restore CA1826 // Do not use Enumerable methods on indexable collections

                var mainPageViewModel = App.Current.Services.GetService<MainPageViewModel>();

                if (filePath != null && mainPageViewModel != null)
                {
                    mainPageViewModel.DbFilePath = filePath;
                    mainPageViewModel.IsActivatedFromFile = true;
                }
            }
        }
    }

}
