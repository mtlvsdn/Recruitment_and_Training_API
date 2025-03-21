using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using MauiUserApp.Views;

namespace MauiUserApp.Views
{
    public partial class CompanyLoginPageViewModel : ObservableObject
    {
        private string _email;
        private string _password;
        private bool _isBusy;
        private readonly HttpClient _httpClient;
        private readonly ILogger<CompanyLoginPageViewModel> _logger;

        public string Email
        {
            get => _email;
            set => SetProperty(ref _email, value);
        }

        public string Password
        {
            get => _password;
            set => SetProperty(ref _password, value);
        }

        public bool IsBusy
        {
            get => _isBusy;
            set => SetProperty(ref _isBusy, value);
        }

        public CompanyLoginPageViewModel(ILogger<CompanyLoginPageViewModel> logger)
        {
            _httpClient = new HttpClient();

#if __ANDROID__
            _httpClient.BaseAddress = new Uri("http://10.0.2.2:7287");
#else
            _httpClient.BaseAddress = new Uri("http://localhost:7287");
#endif

            _logger = logger;
        }

        [RelayCommand]
        private async Task LoginAsync()
        {
            _logger.LogInformation("Login button clicked");

            if (string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Emil and Password are required", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                var loginRequestcompany = new LoginRequestCompany
                {
                    Email = Email,
                    Password = Password
                };

                _logger.LogInformation("Sending login request for {Email}", Email);

                var response = await _httpClient.PostAsJsonAsync("/authenticate-user", loginRequestcompany);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    // Store the token for future API calls
                    Preferences.Set("AuthToken", result.Token);
                    Preferences.Set("Email", result.Email);
                    var storedEmail = Preferences.Get("Email", "");
                    _logger.LogInformation("Stored Email: {storedEmail}", storedEmail);

                    // Navigate to the main page
                    await Shell.Current.GoToAsync("//HomePage");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid Email or password", "OK");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.NotFound)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "User not found", "OK");
                }
                else
                {
                    var content = await response.Content.ReadAsStringAsync();
                    await Application.Current.MainPage.DisplayAlert("Error", $"Login failed: {response.StatusCode} - {content}", "OK");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Login failed for {Email}", Email);
                await Application.Current.MainPage.DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public class LoginRequestCompany
        {
            public string Email { get; set; }
            public string Password { get; set; }
        }

        public class AuthResponse
        {
            public string Token { get; set; }
            public string Email { get; set; }
        }
    }
}
