using PwSafeClient.Maui.Pages;
using PwSafeClient.Maui.Pages.DatabaseManagement;

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
		Routing.RegisterRoute(Routes.Settings, typeof(SettingsPage));
		Routing.RegisterRoute(Routes.Vault, typeof(VaultPage));
		Routing.RegisterRoute(Routes.EntryDetails, typeof(EntryDetailsPage));
		Routing.RegisterRoute(Routes.EntryEdit, typeof(EntryEditPage));
	}
}
