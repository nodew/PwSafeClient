using Microsoft.Extensions.Logging;
using Microsoft.Maui.LifecycleEvents;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Databases;
using PwSafeClient.AppCore.Vault;
using PwSafeClient.AppCore.Vault.Creation;
using PwSafeClient.AppCore.Security.Biometrics;
using PwSafeClient.AppCore.Security.Secrets;
using PwSafeClient.Maui.Pages;
using PwSafeClient.Maui.Pages.DatabaseManagement;
using PwSafeClient.Maui.ViewModels;
using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.Services.Security;
#if WINDOWS
using PwSafeClient.Maui.Platforms.Windows.Services;
#endif
#if ANDROID
using PwSafeClient.Maui.Platforms.Android.Services;
#endif

namespace PwSafeClient.Maui;

public static class MauiProgram
{
	public static MauiApp CreateMauiApp()
	{
		var builder = MauiApp.CreateBuilder();
		builder
			.UseMauiApp<App>()
			.ConfigureLifecycleEvents(events =>
			{
#if ANDROID
				events.AddAndroid(android =>
				{
					android.OnStop(activity =>
					{
						var autoLock = Application.Current?.Handler?.MauiContext?.Services?.GetService<AutoLockService>();
						if (autoLock != null)
						{
							_ = autoLock.NotifyBackgroundedAsync();
						}
					});

					android.OnResume(activity =>
					{
						Application.Current?.Handler?.MauiContext?.Services?.GetService<AutoLockService>()?.NotifyActivity();
					});
				});
#endif

#if IOS || MACCATALYST
				events.AddiOS(ios =>
				{
					ios.DidEnterBackground(app =>
					{
						var autoLock = Application.Current?.Handler?.MauiContext?.Services?.GetService<AutoLockService>();
						if (autoLock != null)
						{
							_ = autoLock.NotifyBackgroundedAsync();
						}
					});

					ios.OnActivated(app =>
					{
						Application.Current?.Handler?.MauiContext?.Services?.GetService<AutoLockService>()?.NotifyActivity();
					});
				});
#endif
			})
			.ConfigureFonts(fonts =>
			{
				fonts.AddFont("OpenSans-Regular.ttf", "OpenSansRegular");
				fonts.AddFont("OpenSans-Semibold.ttf", "OpenSansSemibold");
				fonts.AddFont("iconfonts.ttf", "IconFont");
			});

		builder.Services.AddSingleton<IVaultSession, VaultSession>();
		builder.Services.AddSingleton<IAppPaths, MauiAppPaths>();
		builder.Services.AddSingleton<IAppConfigurationStore, FileAppConfigurationStore>();
		builder.Services.AddSingleton<IDatabaseRegistry, DatabaseRegistry>();
		builder.Services.AddSingleton<IVaultCreator, VaultCreator>();
		builder.Services.AddSingleton<IFilePickerService, FilePickerService>();
		builder.Services.AddSingleton<ISaveFileService, NoopSaveFileService>();
		builder.Services.AddSingleton<AutoLockService>();
#if WINDOWS
		builder.Services.AddSingleton<ISecureSecretStore, WindowsCredentialLockerSecretStore>();
		builder.Services.AddSingleton<IBiometricAuthService, WindowsHelloBiometricAuthService>();
		builder.Services.AddSingleton<ISaveFileService, WindowsSaveFileService>();
#elif ANDROID
		builder.Services.AddSingleton<ISecureSecretStore, SecureStorageSecretStore>();
		builder.Services.AddSingleton<IBiometricAuthService, AndroidBiometricAuthService>();
#elif IOS || MACCATALYST
		builder.Services.AddSingleton<ISecureSecretStore, SecureStorageSecretStore>();
		builder.Services.AddSingleton<IBiometricAuthService, AppleBiometricAuthService>();
#else
		builder.Services.AddSingleton<ISecureSecretStore, SecureStorageSecretStore>();
		builder.Services.AddSingleton<IBiometricAuthService, NoopBiometricAuthService>();
#endif

		builder.Services.AddSingleton<AppShell>();

		builder.Services.AddTransient<DatabaseListViewModel>();
		builder.Services.AddTransient<CreateDatabaseViewModel>();
		builder.Services.AddTransient<ChangeMasterPasswordViewModel>();
		builder.Services.AddTransient<BackupRestoreViewModel>();
		builder.Services.AddTransient<ExportDataViewModel>();
		builder.Services.AddTransient<SettingsViewModel>();
		builder.Services.AddTransient<UnlockViewModel>();
		builder.Services.AddTransient<VaultViewModel>();
		builder.Services.AddTransient<EntryDetailsViewModel>();
		builder.Services.AddTransient<EntryEditViewModel>();

		builder.Services.AddTransient<DatabaseListPage>();
		builder.Services.AddTransient<CreateDatabasePage>();
		builder.Services.AddTransient<ChangeMasterPasswordPage>();
		builder.Services.AddTransient<BackupRestorePage>();
		builder.Services.AddTransient<ExportDataPage>();
		builder.Services.AddTransient<SettingsPage>();
		builder.Services.AddTransient<UnlockPage>();
		builder.Services.AddTransient<VaultPage>();
		builder.Services.AddTransient<EntryDetailsPage>();
		builder.Services.AddTransient<EntryEditPage>();

#if DEBUG
		builder.Logging.AddDebug();
#endif

		return builder.Build();
	}
}
