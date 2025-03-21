using MauiClientApp.Services;
using MauiClientApp.ViewModels;

namespace MauiClientApp.Views;

public partial class DashboardPage : ContentPage
{
    public DashboardPage()
    {
        InitializeComponent();
        BindingContext = new DashboardViewModel();

        // Display the company name
        var companyName = SessionManager.Instance.CompanyName;
        var email = SessionManager.Instance.Email;

        CompanyLabel.Text = $"Company: {companyName}\nEmail: {email}";
    }
}