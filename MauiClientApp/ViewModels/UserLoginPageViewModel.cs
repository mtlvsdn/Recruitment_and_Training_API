using MauiClientApp.Services;
using MauiClientApp.Views;
using MauiClientApp.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class UserLoginPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _email;
        private string _password;
        private bool _isLoading;
        private string _errorMessage;

        public string Email
        {
            get => _email;
            set
            {
                if (_email != value)
                {
                    _email = value;
                    OnPropertyChanged();
                }
            }
        }

        public string Password
        {
            get => _password;
            set
            {
                if (_password != value)
                {
                    _password = value;
                    OnPropertyChanged();
                }
            }
        }

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

        public string ErrorMessage
        {
            get => _errorMessage;
            set
            {
                if (_errorMessage != value)
                {
                    _errorMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand LoginCommand { get; }

        public UserLoginPageViewModel()
        {
            _apiService = new ApiService();
            LoginCommand = new Command(async () => await LoginAsync());
        }

        private async Task LoginAsync()
        {
            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                ErrorMessage = "Please enter both email and password";
                return;
            }

            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                var loginData = new { Email = Email, Password = Password };
                var response = await _apiService.PostAsync<AuthResponse>("authenticate-user", loginData);

                if (!string.IsNullOrEmpty(response.Token))
                {
                    // Store session information
                    SessionManager.Instance.SetSession(response.Token, response.Email);

                    // Get user details to store user's full name
                    var user = await _apiService.GetAsync<User>($"user/byemail/{Email}");
                    if (user != null)
                    {
                        SessionManager.Instance.SetUserDetails(user.Full_Name, user.Company_Name, user.Id);
                    }

                    // Navigate to the user dashboard instead of the company dashboard
                    Application.Current.MainPage = new NavigationPage(new UserDashboardPage());
                }
                else
                {
                    ErrorMessage = "Invalid login credentials";
                }
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Login failed: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // This class should match the response from the authenticate-user endpoint
    public class AuthResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}