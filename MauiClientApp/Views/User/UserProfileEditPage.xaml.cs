using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.User;

public partial class UserProfileEditPage : ContentPage
{
	public UserProfileEditPage()
	{
		InitializeComponent();
        BindingContext = new UserProfileEditViewModel();
    }
} 