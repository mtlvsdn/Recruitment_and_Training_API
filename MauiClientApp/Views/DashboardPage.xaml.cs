using MauiClientApp.Services;
using MauiClientApp.ViewModels;
using MauiClientApp.Models;

namespace MauiClientApp.Views;

public partial class DashboardPage : ContentPage
{
    private readonly ApiService _apiService;
    private readonly SessionManager _sessionManager;
    private DashboardViewModel _viewModel;

    public DashboardPage()
    {
        InitializeComponent();
        _viewModel = new DashboardViewModel();
        BindingContext = _viewModel;
        _apiService = new ApiService();
        _sessionManager = SessionManager.Instance;

        // Initialize company information
        InitializeCompanyInfo();
    }

    protected override void OnAppearing()
    {
        base.OnAppearing();
        // No auto-login popup needed
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

    private async void OnLogoutClicked(object sender, EventArgs e)
    {
        bool confirm = await DisplayAlert("Confirm Logout", 
            "Do you want to clear your saved credentials? This will require you to login again next time.", 
            "Yes", "No");
            
        if (confirm)
        {
            SessionManager.Instance.ClearSession();
            Application.Current.MainPage = new NavigationPage(new StartUpPage());
        }
    }
}