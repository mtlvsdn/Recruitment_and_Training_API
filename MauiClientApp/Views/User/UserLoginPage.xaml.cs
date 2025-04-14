using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.User;

public partial class UserLoginPage : ContentPage
{
    private UserLoginPageViewModel _viewModel;

    public UserLoginPage()
    {
        InitializeComponent();
        _viewModel = new UserLoginPageViewModel();
        BindingContext = _viewModel;
        
        // Add a tooltip or indication about credentials being saved automatically
        this.Loaded += (s, e) => 
        {
            // Could display a tooltip or message indicating auto-login is enabled
            System.Diagnostics.Debug.WriteLine("User login page loaded - credentials will be saved for future auto-login");
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