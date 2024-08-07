using CommunityToolkit.Maui;

using MauiIcons.FontAwesome;
using MauiIcons.Material;

using Microsoft.Extensions.Logging;

using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui
{
    public static class MauiProgram
    {
        public static MauiApp CreateMauiApp()
        {
            var builder = MauiApp.CreateBuilder();
            builder
                .UseMauiApp<App>()
                .UseMauiCommunityToolkit()
                .UseFontAwesomeMauiIcons()
                .UseMaterialMauiIcons()
                .ConfigureFonts(fonts =>
                {
                    fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
                    fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
                });

#if DEBUG
            builder.Logging.AddDebug();
#endif

            #region ViewModels
            builder.Services.AddSingleton<AppShellViewModel>();
            builder.Services.AddSingleton<MainPageViewModel>();

            builder.Services.AddScoped<SettingsPageViewModel>();
            #endregion

            return builder.Build();
        }
    }
}
