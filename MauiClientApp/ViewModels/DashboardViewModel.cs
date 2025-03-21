using MauiClientApp.Services;
using MauiClientApp.Views;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class DashboardViewModel
    {
        public ICommand LogoutCommand { get; }

        public DashboardViewModel()
        {
            LogoutCommand = new Command(Logout);
        }

        private async void Logout()
        {
            // Clear the session
            SessionManager.Instance.ClearSession();

            // Navigate back to the login page
            Application.Current.MainPage = new NavigationPage(new StartUpPage());
        }
    }
}