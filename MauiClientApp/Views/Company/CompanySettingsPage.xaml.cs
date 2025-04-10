using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.Company;

public partial class CompanySettingsPage : ContentPage
{
	public CompanySettingsPage()
	{
		InitializeComponent();
        BindingContext = new CompanySettingsViewModel();
    }
} 