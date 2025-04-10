using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.User;

public partial class UserSkillsPage : ContentPage
{
	public UserSkillsPage()
	{
		InitializeComponent();
        BindingContext = new UserSkillsViewModel();
    }
} 