using MauiClientApp.Services;
using MauiClientApp.Views;
using MauiClientApp.Views.Company;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class CompanyLoginPageViewModel : INotifyPropertyChanged
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

        public CompanyLoginPageViewModel()
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
                var response = await _apiService.PostAsync<AuthResponse>("authenticate-company", loginData);

                if (!string.IsNullOrEmpty(response.Token))
                {
                    // Get company details
                    var company = await _apiService.GetAsync<Company>($"company/byemail/{Email}");
                    if (company != null)
                    {
                        // Store session information for company
                        SessionManager.Instance.SetSession(response.Token, response.Email, company.Company_Name);
                    }
                    else
                    {
                        SessionManager.Instance.SetSession(response.Token, response.Email);
                    }

                    // Navigate to the company dashboard
                    Application.Current.MainPage = new NavigationPage(new DashboardPage());
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

    public class Company
    {
        public string Company_Name { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public int Nr_of_accounts { get; set; }
        public string SuperUseremail { get; set; }
    }
}