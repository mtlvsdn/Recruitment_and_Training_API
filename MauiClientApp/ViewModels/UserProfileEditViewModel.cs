using MauiClientApp.Models;
using MauiClientApp.Services;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace MauiClientApp.ViewModels
{
    public partial class UserProfileEditViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly SessionManager _sessionManager;
        private User _user;

        [ObservableProperty]
        private string fullName;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private string companyName;

        [ObservableProperty]
        private bool isLoading;

        public ICommand UpdateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand InitializeCommand { get; }

        public UserProfileEditViewModel()
        {
            _apiService = new ApiService();
            _sessionManager = SessionManager.Instance;
            
            UpdateCommand = new AsyncRelayCommand(UpdateProfileAsync);
            CancelCommand = new AsyncRelayCommand(GoBackAsync);
            InitializeCommand = new AsyncRelayCommand(InitializeAsync);
            
            // Load user data when view model is created
            Task.Run(() => InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                
                // Get current user data from the API
                _user = await _apiService.GetAsync<User>($"user/{_sessionManager.UserId}");
                
                if (_user != null)
                {
                    // Populate the form fields
                    FullName = _user.Full_Name;
                    Email = _user.Email;
                    Password = _user.Password;
                    CompanyName = _user.Company_Name;
                }
                else
                {
                    // Fallback to session data if API call fails
                    FullName = _sessionManager.UserFullName;
                    Email = _sessionManager.Email;
                    CompanyName = _sessionManager.CompanyName;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load profile: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateProfileAsync()
        {
            if (string.IsNullOrWhiteSpace(FullName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", "Please fill in all required fields", "OK");
                return;
            }

            try
            {
                IsLoading = true;

                // Create updated user object
                var updatedUser = new User
                {
                    Id = _sessionManager.UserId,
                    Full_Name = FullName,
                    Email = Email,
                    Password = Password,
                    Company_Name = CompanyName // This won't change
                };

                // Update user in the database
                await _apiService.PutAsync<User>($"user/{_sessionManager.UserId}", updatedUser);
                
                // Update session data
                _sessionManager.SetUserDetails(FullName, CompanyName, _sessionManager.UserId);
                _sessionManager.SetSession(_sessionManager.Token, Email, CompanyName);

                await Application.Current.MainPage.DisplayAlert("Success", "Profile updated successfully", "OK");
                
                // Go back to dashboard
                await GoBackAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to update profile: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GoBackAsync()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
} 