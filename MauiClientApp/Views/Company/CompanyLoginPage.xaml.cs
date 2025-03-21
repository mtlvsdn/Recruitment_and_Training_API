using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.Company;

public partial class CompanyLoginPage : ContentPage
{
    private CompanyLoginPageViewModel _viewModel;

    public CompanyLoginPage()
    {
        InitializeComponent();
        _viewModel = BindingContext as CompanyLoginPageViewModel;
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