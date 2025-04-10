using MauiClientApp.Services;
using MauiClientApp.ViewModels;
using MauiClientApp.Models;

namespace MauiClientApp.Views;

public partial class DashboardPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly SessionManager _sessionManager;

    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = new DashboardViewModel();
        _apiService = new ApiService();
        _sessionManager = SessionManager.Instance;

        // Initialize company information
        InitializeCompanyInfo();
    }

    private async void InitializeCompanyInfo()
    {
        try
        {
            // Get company details from the API
            var company = await _apiService.GetAsync<Models.Company>($"company/{_sessionManager.CompanyName}");
            
            if (company != null)
            {
                // Set the email label
                EmailLabel.Text = company.Email;

                // Count the number of users associated with this company
                var users = await _apiService.GetListAsync<Models.User>("user");
                var userCount = users?.Count(u => u.Company_Name == _sessionManager.CompanyName) ?? 0;
                AccountsLabel.Text = userCount.ToString();
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error loading company information: {ex.Message}");
            // Set default values if there's an error
            EmailLabel.Text = "Error loading email";
            AccountsLabel.Text = "0";
        }
    }
}