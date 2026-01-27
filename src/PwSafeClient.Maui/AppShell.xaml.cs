using Microsoft.Extensions.DependencyInjection;
using Microsoft.Maui;

using PwSafeClient.Maui.Pages;
using PwSafeClient.Maui.Pages.DatabaseManagement;
using PwSafeClient.Maui.Pages.Settings;

namespace PwSafeClient.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

		Routing.RegisterRoute(Routes.DatabaseList, new ServiceRouteFactory(typeof(DatabaseListPage)));
		Routing.RegisterRoute(Routes.CreateDatabase, new ServiceRouteFactory(typeof(CreateDatabasePage)));
		Routing.RegisterRoute(Routes.ChangeMasterPassword, new ServiceRouteFactory(typeof(ChangeMasterPasswordPage)));
		Routing.RegisterRoute(Routes.BackupRestore, new ServiceRouteFactory(typeof(BackupRestorePage)));
		Routing.RegisterRoute(Routes.ExportData, new ServiceRouteFactory(typeof(ExportDataPage)));
		Routing.RegisterRoute(Routes.CloudSync, new ServiceRouteFactory(typeof(CloudSyncPage)));
		Routing.RegisterRoute(Routes.PasswordPolicies, new ServiceRouteFactory(typeof(PasswordPoliciesPage)));
		Routing.RegisterRoute(Routes.About, new ServiceRouteFactory(typeof(AboutPage)));
		Routing.RegisterRoute(Routes.PrivacyPolicy, new ServiceRouteFactory(typeof(PrivacyPolicyPage)));
		Routing.RegisterRoute(Routes.TermsOfService, new ServiceRouteFactory(typeof(TermsOfServicePage)));
		Routing.RegisterRoute(Routes.Settings, new ServiceRouteFactory(typeof(SettingsPage)));
		Routing.RegisterRoute(Routes.Vault, new ServiceRouteFactory(typeof(VaultPage)));
		Routing.RegisterRoute(Routes.EntryDetails, new ServiceRouteFactory(typeof(EntryDetailsPage)));
		Routing.RegisterRoute(Routes.EntryEdit, new ServiceRouteFactory(typeof(EntryEditPage)));
	}

	private sealed class ServiceRouteFactory : RouteFactory
	{
		private readonly Type _type;

		public ServiceRouteFactory(Type type)
		{
			_type = type;
		}

		public override Element GetOrCreate(IServiceProvider services)
		{
			return (Element)(services.GetService(_type)
				?? ActivatorUtilities.CreateInstance(services, _type));
		}

		public override Element GetOrCreate()
		{
			var services = Application.Current?.Handler?.MauiContext?.Services;
			if (services == null)
			{
				throw new InvalidOperationException("Service provider is not available for route navigation.");
			}

			return GetOrCreate(services);
		}
    }
}
