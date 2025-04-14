using MauiClientApp.Services;
using MauiClientApp.ViewModels;

namespace MauiClientApp.Views
{
    public partial class UserDashboardPage : ContentPage
    {
        private UserDashboardViewModel _viewModel;
        private SessionManager _sessionManager;

        public UserDashboardPage()
        {
            InitializeComponent();
            _viewModel = new UserDashboardViewModel();
            BindingContext = _viewModel;
            _sessionManager = SessionManager.Instance;

            // Set label texts based on session information
            if (_sessionManager.IsUserLogin)
            {
                UserLabel.Text = $"Welcome, {_sessionManager.UserFullName}";
                UserCompanyLabel.Text = $"Company: {_sessionManager.CompanyName}";
                EmailLabel.Text = _sessionManager.Email;
                CompanyLabel.Text = _sessionManager.CompanyName;
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            // Refresh data if needed when page appears
            _viewModel.RefreshData();
        }
        
        private async void OnLogoutClicked(object sender, EventArgs e)
        {
            bool confirm = await DisplayAlert("Confirm Logout", 
                "Do you want to clear your saved credentials? This will require you to login again next time.", 
                "Yes", "No");
                
            if (confirm)
            {
                SessionManager.Instance.ClearSession();
                Application.Current.MainPage = new NavigationPage(new StartUpPage());
            }
        }
    }
}