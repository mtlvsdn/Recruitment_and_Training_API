using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;
using System;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public partial class CompanySettingsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
        private readonly SessionManager _sessionManager;

        [ObservableProperty]
        private string companyName;

        [ObservableProperty]
        private string email;

        [ObservableProperty]
        private string password;

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private string statusMessage;

        public ICommand UpdateCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand InitializeCommand { get; }

        private Company _originalCompany;

        public CompanySettingsViewModel()
        {
            _apiService = new ApiService();
            _sessionManager = SessionManager.Instance;
            
            UpdateCommand = new AsyncRelayCommand(UpdateCompanyAsync);
            CancelCommand = new AsyncRelayCommand(GoBackAsync);
            InitializeCommand = new AsyncRelayCommand(InitializeAsync);
            
            // Load company data when ViewModel is created
            Task.Run(() => InitializeAsync());
        }

        private async Task InitializeAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "";
                
                // Get company data from the API
                var company = await _apiService.GetAsync<Company>($"company/{_sessionManager.CompanyName}");
                
                if (company != null)
                {
                    // Populate the form fields with API data
                    CompanyName = company.Company_Name;
                    Email = company.Email;
                    Password = company.Password;
                    
                    // Store these values for later use
                    _originalCompany = company;
                }
                else
                {
                    // Fallback to session data if API call fails
                    CompanyName = _sessionManager.CompanyName;
                    Email = _sessionManager.Email;
                    Password = ""; // Can't retrieve password from session
                    StatusMessage = "Could not retrieve company details from server. Some data may be incomplete.";
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading company details: {ex.Message}";
                
                // Use session data as fallback
                CompanyName = _sessionManager.CompanyName;
                Email = _sessionManager.Email;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task UpdateCompanyAsync()
        {
            if (string.IsNullOrWhiteSpace(CompanyName) || string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(Password))
            {
                StatusMessage = "Please fill in all required fields";
                return;
            }

            try
            {
                IsLoading = true;

                // Create updated company object
                var updatedCompany = new Company
                {
                    Company_Name = CompanyName,
                    Email = Email,
                    Password = Password,
                    // Keep the original values for these fields
                    Nr_of_accounts = _originalCompany?.Nr_of_accounts ?? 0,
                    SuperUseremail = _originalCompany?.SuperUseremail ?? ""
                };

                // Update company in the database
                await _apiService.PutAsync<Company>($"company/{_sessionManager.CompanyName}", updatedCompany);
                
                // Update session data
                _sessionManager.SetSession(_sessionManager.Token, Email, CompanyName);

                await Application.Current.MainPage.DisplayAlert("Success", "Company information updated successfully", "OK");
                
                // Go back to dashboard
                await GoBackAsync();
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error updating company: {ex.Message}";
                Console.WriteLine($"Update company error details: {ex}");
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