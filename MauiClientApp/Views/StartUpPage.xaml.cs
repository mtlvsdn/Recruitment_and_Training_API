using MauiClientApp.Services;
using MauiClientApp.Views.Company;
using MauiClientApp.Views.User;

namespace MauiClientApp.Views;

public partial class StartUpPage : ContentPage
{
	public StartUpPage()
	{
		InitializeComponent();
        CheckSavedLogin();
	}

    private async void CheckSavedLogin()
    {
        try
        {
            bool hasSession = await SessionManager.Instance.LoadSessionAsync();
            if (hasSession)
            {
                // Delay navigation slightly to ensure UI is loaded
                await Task.Delay(100);
                
                if (SessionManager.Instance.IsUserLogin)
                {
                    await Navigation.PushAsync(new UserDashboardPage());
                }
                else if (SessionManager.Instance.IsCompanyLogin)
                {
                    await Navigation.PushAsync(new DashboardPage());
                }
            }
        }
        catch (Exception ex)
        {
            // Just log the error and continue with normal startup
            System.Diagnostics.Debug.WriteLine($"Error loading saved login: {ex.Message}");
        }
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