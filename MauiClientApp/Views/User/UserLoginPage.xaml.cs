using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.User;

public partial class UserLoginPage : ContentPage
{
    private UserLoginPageViewModel _viewModel;

    public UserLoginPage()
    {
        InitializeComponent();
        _viewModel = BindingContext as UserLoginPageViewModel;
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