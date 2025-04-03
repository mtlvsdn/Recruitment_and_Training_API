using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.User;

public partial class UserCvPage : ContentPage
{
	public UserCvPage()
	{
		InitializeComponent();
        BindingContext = new UserCvViewModel();
    }
}