using Android.App;
using Android.Content.PM;
using Android.OS;

namespace PwSafeClient.Maui
{
    [Activity(Theme = "@style/Maui.SplashTheme", MainLauncher = true, LaunchMode = LaunchMode.SingleTop, ConfigurationChanges = ConfigChanges.ScreenSize | ConfigChanges.Orientation | ConfigChanges.UiMode | ConfigChanges.ScreenLayout | ConfigChanges.SmallestScreenSize | ConfigChanges.Density)]
    public class MainActivity : MauiAppCompatActivity
    {
        public override void OnCreate(Bundle? savedInstanceState, PersistableBundle? persistentState)
        {
            base.OnCreate(savedInstanceState, persistentState);

            if (Intent?.Data != null)
            {
                var filePath = Intent.Data.Path;

                if (App.Current?.MainPage is AppShell mainPage && filePath != null)
                {
                    mainPage.SetDatabaseFile(filePath);
                }
            }
        }
    }
}
