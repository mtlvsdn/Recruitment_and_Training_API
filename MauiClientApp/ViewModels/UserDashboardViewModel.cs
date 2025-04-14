using MauiClientApp.Services;
using MauiClientApp.Views;
using MauiClientApp.Views.User;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class UserDashboardViewModel : INotifyPropertyChanged
    {
        private readonly SessionManager _sessionManager;
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LogoutCommand { get; }
        public ICommand ViewTestsCommand { get; }
        public ICommand UploadCVCommand { get; }
        public ICommand ViewSkillsCommand { get; }
        public ICommand EditProfileCommand { get; }

        public UserDashboardViewModel()
        {
            _sessionManager = SessionManager.Instance;

            LogoutCommand = new Command(Logout);
            ViewTestsCommand = new Command(ViewTests);
            UploadCVCommand = new Command(UploadCV);
            ViewSkillsCommand = new Command(ViewSkills);
            EditProfileCommand = new Command(EditProfile);
        }

        public void RefreshData()
        {
            // Refresh any data that needs to be updated when returning to this page
            // This could involve API calls to get updated user information
        }

        private void Logout()
        {
            _sessionManager.ClearSession();
            Application.Current.MainPage = new NavigationPage(new StartUpPage());
        }

        private async void ViewTests()
        {
            // Navigate to tests page
            await Application.Current.MainPage.Navigation.PushAsync(new Views.User.UserTestsPage());
        }

        private async void UploadCV()
        {
            await Application.Current.MainPage.Navigation.PushAsync(new UserCvPage());
        }

        private void ViewSkills()
        {
            // Navigate to skills page
            // Example: Application.Current.MainPage.Navigation.PushAsync(new SkillsPage());
        }

        private async void EditProfile()
        {
            // Navigate to profile edit page
            await Application.Current.MainPage.Navigation.PushAsync(new Views.User.UserProfileEditPage());
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}