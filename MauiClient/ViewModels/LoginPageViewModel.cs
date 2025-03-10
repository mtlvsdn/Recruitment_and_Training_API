using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;

namespace MauiClient.ViewModels
{
    public partial class LoginPageViewModel : ObservableObject
    {
        private string _superUseremail;
        private string _password;
        private bool _isBusy;
        private readonly HttpClient _httpClient;
        private readonly ILogger<LoginPageViewModel> _logger;

        public string SuperUseremail
        {
            get => _superUseremail;
            set => SetProperty(ref _superUseremail, value);
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

        public LoginPageViewModel(ILogger<LoginPageViewModel> logger)
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

            if (string.IsNullOrWhiteSpace(SuperUseremail) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "SuperUseremail and password are required", "OK");
                return;
            }

            try
            {
                IsBusy = true;
                var loginRequest = new LoginRequest
                {
                    SuperUseremail = SuperUseremail,
                    Password = Password
                };

                _logger.LogInformation("Sending login request for {SuperUseremail}", SuperUseremail);

                var response = await _httpClient.PostAsJsonAsync("/authenticate", loginRequest);

                if (response.IsSuccessStatusCode)
                {
                    var result = await response.Content.ReadFromJsonAsync<AuthResponse>();
                    // Store the token for future API calls
                    Preferences.Set("AuthToken", result.Token);
                    Preferences.Set("SuperUseremail", result.SuperUseremail);
                    var storedEmail = Preferences.Get("SuperUseremail", "");
                    _logger.LogInformation("Stored SuperUseremail: {storedEmail}", storedEmail);

                    // Navigate to the main page
                    await Shell.Current.GoToAsync("//HomePage");
                }
                else if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid SuperUseremail or password", "OK");
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
                _logger.LogError(ex, "Login failed for {SuperUseremail}", SuperUseremail);
                await Application.Current.MainPage.DisplayAlert("Error", $"Login failed: {ex.Message}", "OK");
            }
            finally
            {
                IsBusy = false;
            }
        }

        public class LoginRequest
        {
            public string SuperUseremail { get; set; }
            public string Password { get; set; }
        }

        public class AuthResponse
        {
            public string Token { get; set; }
            public string SuperUseremail { get; set; }
        }
    }
}
