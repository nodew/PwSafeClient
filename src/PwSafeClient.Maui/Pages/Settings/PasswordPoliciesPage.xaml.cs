using PwSafeClient.Maui.Services;
using PwSafeClient.Maui.ViewModels;

namespace PwSafeClient.Maui.Pages.Settings;

    public partial class PasswordPoliciesPage : ContentPage
    {
        private readonly AutoLockService _autoLock;
        private readonly PasswordPoliciesViewModel _viewModel;

        public PasswordPoliciesPage(PasswordPoliciesViewModel viewModel, AutoLockService autoLock)
        {
            InitializeComponent();
            BindingContext = viewModel;
            _autoLock = autoLock;
            _viewModel = viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _autoLock.NotifyActivity();
            _viewModel.Refresh();
        }
    }
