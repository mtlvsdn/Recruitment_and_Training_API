using MauiClient.ViewModels;

namespace MauiClient
{
    public partial class LoginPage : ContentPage
    {
        public LoginPage(LoginPageViewModel viewModel)
        {
            InitializeComponent();
            BindingContext = viewModel;
        }
    }
}