using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.Company;

public partial class CompanyLoginPage : ContentPage
{
    private CompanyLoginPageViewModel _viewModel;

    public CompanyLoginPage()
    {
        InitializeComponent();
        _viewModel = new CompanyLoginPageViewModel();
        BindingContext = _viewModel;
        
        // Add a tooltip or indication about credentials being saved automatically
        this.Loaded += (s, e) => 
        {
            // Could display a tooltip or message indicating auto-login is enabled
            System.Diagnostics.Debug.WriteLine("Company login page loaded - credentials will be saved for future auto-login");
        };
    }

    // This method is kept for compatibility with the original code
    // but we're now using the ViewModel's LoginCommand
    private void OnLoginButtonTapped(object sender, EventArgs e)
    {
        if (_viewModel != null)
        {
            _viewModel.LoginCommand.Execute(null);
        }
    }
}