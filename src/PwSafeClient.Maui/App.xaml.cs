namespace PwSafeClient.Maui;

using System;
using System.IO;
using System.Threading.Tasks;

using PwSafeClient.AppCore.Configuration;
using PwSafeClient.AppCore.Databases;
using PwSafeClient.Maui.Services;

public partial class App : Application
{
	private readonly AppShell _appShell;
	private readonly AutoLockService _autoLock;
	private readonly IAppConfigurationStore _configStore;
	private readonly IDatabaseRegistry _databaseRegistry;
	private bool _initialNavigationCompleted;

	public App(
		AppShell appShell,
		AutoLockService autoLock,
		IAppConfigurationStore configStore,
		IDatabaseRegistry databaseRegistry)
	{
		InitializeComponent();
		_appShell = appShell;
		_autoLock = autoLock;
		_configStore = configStore;
		_databaseRegistry = databaseRegistry;

		_ = ApplyThemeFromConfigAsync();
	}

	private async Task ApplyThemeFromConfigAsync()
	{
		try
		{
			var config = await _configStore.LoadAsync();
			ApplyTheme(config.Theme);
		}
		catch
		{
			// ignore; theming is best-effort at startup
		}
	}

	private static void ApplyTheme(AppThemePreference preference)
	{
		if (Current == null)
		{
			return;
		}

		Current.UserAppTheme = preference switch
		{
			AppThemePreference.System => AppTheme.Unspecified,
			AppThemePreference.Dark => AppTheme.Dark,
			AppThemePreference.Light => AppTheme.Light,
			_ => AppTheme.Unspecified
		};
	}

	protected override Window CreateWindow(IActivationState? activationState)
	{
		var window = new Window(_appShell);

		window.Activated += (_, _) => _autoLock.NotifyActivity();
		window.Stopped += async (_, _) => await _autoLock.NotifyBackgroundedAsync();

		// Best-effort initial routing. AppShell defaults to DatabaseList; if a default database exists
		// and its file is available, start on Unlock instead.
		MainThread.BeginInvokeOnMainThread(async () => await NavigateOnStartupAsync());

		return window;
	}

	private async Task NavigateOnStartupAsync()
	{
		if (_initialNavigationCompleted)
		{
			return;
		}

		_initialNavigationCompleted = true;

		try
		{
			AppConfiguration config;
			try
			{
				config = await _configStore.LoadAsync();
			}
			catch
			{
				return;
			}

			var alias = config.DefaultDatabase;
			if (string.IsNullOrWhiteSpace(alias))
			{
				return;
			}

			var path = await _databaseRegistry.TryGetPathAsync(alias);
			if (string.IsNullOrWhiteSpace(path) || !File.Exists(path))
			{
				return;
			}

			// Shell.Current can be null briefly during startup depending on platform timing.
			for (var i = 0; i < 20 && Shell.Current == null; i++)
			{
				await Task.Delay(25);
			}

			if (Shell.Current == null)
			{
				return;
			}

			var encodedAlias = Uri.EscapeDataString(alias);
			await Shell.Current.GoToAsync($"//{Routes.DatabaseList}/{Routes.Unlock}?alias={encodedAlias}");
		}
		catch
		{
			// ignore; startup navigation is best-effort
		}
	}
}
