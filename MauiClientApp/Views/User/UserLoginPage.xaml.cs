namespace MauiClientApp.Views.User;

public partial class UserLoginPage : ContentPage
{
	public UserLoginPage()
	{
		InitializeComponent();
	}

    private async void OnLoginButtonTapped(object sender, EventArgs e)
    {
        // Add login logic here, such as validating email/password.
        // For now, just show a message:
        await DisplayAlert("Login", "Login button clicked", "OK");
    }
}