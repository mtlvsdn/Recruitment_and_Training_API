using MauiClientApp.Services;
using MauiClientApp.Views;
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

        public DashboardViewModel()
        {
            _sessionManager = SessionManager.Instance;
            LogoutCommand = new Command(Logout);
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