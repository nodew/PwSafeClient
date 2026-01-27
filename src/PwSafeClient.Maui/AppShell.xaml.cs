using PwSafeClient.Maui.Pages;
using PwSafeClient.Maui.Pages.DatabaseManagement;
using PwSafeClient.Maui.Pages.Settings;

namespace PwSafeClient.Maui;

public partial class AppShell : Shell
{
	public AppShell()
	{
		InitializeComponent();

        Routing.RegisterRoute(Routes.DatabaseList, typeof(DatabaseListPage));
		Routing.RegisterRoute(Routes.CreateDatabase, typeof(CreateDatabasePage));
		Routing.RegisterRoute(Routes.ChangeMasterPassword, typeof(ChangeMasterPasswordPage));
		Routing.RegisterRoute(Routes.BackupRestore, typeof(BackupRestorePage));
		Routing.RegisterRoute(Routes.ExportData, typeof(ExportDataPage));
		Routing.RegisterRoute(Routes.CloudSync, typeof(CloudSyncPage));
		Routing.RegisterRoute(Routes.PasswordPolicies, typeof(PasswordPoliciesPage));
		Routing.RegisterRoute(Routes.About, typeof(AboutPage));
		Routing.RegisterRoute(Routes.PrivacyPolicy, typeof(PrivacyPolicyPage));
		Routing.RegisterRoute(Routes.TermsOfService, typeof(TermsOfServicePage));
		Routing.RegisterRoute(Routes.Settings, typeof(SettingsPage));
		Routing.RegisterRoute(Routes.Vault, typeof(VaultPage));
		Routing.RegisterRoute(Routes.EntryDetails, typeof(EntryDetailsPage));
		Routing.RegisterRoute(Routes.EntryEdit, typeof(EntryEditPage));
	}
}
