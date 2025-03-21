using MauiClientApp.Services;
using MauiClientApp.ViewModels;

namespace MauiClientApp.Views
{
    public partial class DashboardPage : ContentPage
    {
        private DashboardViewModel _viewModel;
        private SessionManager _sessionManager;

        public DashboardPage()
        {
            InitializeComponent();

            _viewModel = new DashboardViewModel();
            BindingContext = _viewModel;

            _sessionManager = SessionManager.Instance;

            // Set label texts based on session information
            if (_sessionManager.IsCompanyLogin)
            {
                CompanyLabel.Text = $"Company: {_sessionManager.CompanyName}";
            }
            else if (_sessionManager.IsUserLogin)
            {
                UserLabel.Text = $"Welcome, {_sessionManager.UserFullName}";
                UserCompanyLabel.Text = $"Company: {_sessionManager.CompanyName}";
            }
        }
    }
}