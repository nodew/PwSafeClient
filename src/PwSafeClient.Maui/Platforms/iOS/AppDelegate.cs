using Foundation;

using UIKit;

namespace PwSafeClient.Maui
{
    [Register("AppDelegate")]
    public class AppDelegate : MauiUIApplicationDelegate
    {
        protected override MauiApp CreateMauiApp() => MauiProgram.CreateMauiApp();

        public override bool OpenUrl(UIApplication app, NSUrl url, NSDictionary options)
        {
            var filePath = url.Path;

            if (App.Current?.MainPage is AppShell mainPage && filePath != null)
            {
                mainPage.SetDatabaseFile(filePath);
            }

            return true;
        }
    }
}
