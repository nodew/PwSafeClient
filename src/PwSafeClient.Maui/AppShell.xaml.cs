using PwSafeClient.Maui.ViewModels;
using PwSafeClient.Maui.Views;

namespace PwSafeClient.Maui
{
    public partial class AppShell : Shell
    {
        private readonly IServiceProvider services;
        private readonly AppShellViewModel viewModel;

        public AppShell(IServiceProvider services)
        {
            this.services = services;

            viewModel = services.GetRequiredService<AppShellViewModel>();

            InitializeComponent();

            BindingContext = viewModel;

            Routing.RegisterRoute(nameof(PasswordListPage), typeof(PasswordListPage));
        }

        public void SetDatabaseFile(string filePath)
        {
            var mainPageViewModel = services.GetRequiredService<MainPageViewModel>();

            mainPageViewModel.DbFilePath = filePath;
            mainPageViewModel.IsActivatedFromFile = true;
        }
    }
}
