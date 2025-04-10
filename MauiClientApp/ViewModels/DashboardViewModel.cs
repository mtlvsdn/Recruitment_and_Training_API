using MauiClientApp.Services;
using MauiClientApp.Views;
using MauiClientApp.Views.Company;
using MauiClientApp.Views.Tests;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class DashboardViewModel : INotifyPropertyChanged
    {
        private readonly SessionManager _sessionManager;

        public string CompanyName => _sessionManager.CompanyName;
        public string UserFullName => _sessionManager.UserFullName;
        public bool IsCompanyLogin => _sessionManager.IsCompanyLogin;
        public bool IsUserLogin => _sessionManager.IsUserLogin;

        public ICommand LogoutCommand { get; }
        public ICommand ManageUsersCommand { get; }
        public ICommand CreateTestsCommand { get; }
        public ICommand ViewTestsCommand { get; }
        public ICommand ViewAnalyticsCommand { get; }
        public ICommand CompanySettingsCommand { get; }

        public DashboardViewModel()
        {
            _sessionManager = SessionManager.Instance;

            ManageUsersCommand = new Command(OpenManageUsers);
            CreateTestsCommand = new Command(OpenCreateTests);
            ViewTestsCommand = new Command(async () => await OnViewTests());
            ViewAnalyticsCommand = new Command(OpenViewAnalytics);
            CompanySettingsCommand = new Command(OpenCompanySettings);
            LogoutCommand = new Command(Logout);
        }

        private async void OpenManageUsers()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ManageUsers());
        }

        private async void OpenCreateTests()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new CreateTestPage());
        }

        private async Task OnViewTests()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new TestsPage());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Navigation Error", $"Failed to navigate to Tests page: {ex.Message}", "OK");
            }
        }

        private async void OpenViewAnalytics()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new ViewAnalyticsPage());
        }

        private async void OpenCompanySettings()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PushAsync(new CompanySettingsPage());
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Navigation Error", $"Failed to navigate to Company Settings: {ex.Message}", "OK");
            }
        }

        private async void Logout()
        {
            // Clear the session
            _sessionManager.ClearSession();

            // Navigate back to the login page
            Application.Current.MainPage = new NavigationPage(new StartUpPage());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}