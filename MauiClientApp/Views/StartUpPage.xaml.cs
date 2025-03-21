using MauiClientApp.Views.Company;
using MauiClientApp.Views.User;

namespace MauiClientApp.Views;

public partial class StartUpPage : ContentPage
{
	public StartUpPage()
	{
		InitializeComponent();
	}

    private async void OnCompanyLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new CompanyLoginPage());
    }

    private async void OnUserLoginTapped(object sender, EventArgs e)
    {
        await Navigation.PushAsync(new UserLoginPage());
    }
}