namespace MauiUserApp.Views;

public partial class UserLoginPage : ContentPage
{
	public UserLoginPage(UserLoginPageViewModel viewModel)
	{
		InitializeComponent();
        BindingContext = viewModel;
    }
}