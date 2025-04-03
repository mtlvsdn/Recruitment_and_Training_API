using MauiClientApp.ViewModels;
using MauiClientApp.Models;
using Microsoft.Maui.Controls;

namespace MauiClientApp.Views.Company
{
    public partial class EditUserPage : ContentPage
    {
        // Constructor expects a UserModel parameter with the data of the user to edit.
        public EditUserPage(UserModel user)
        {
            InitializeComponent();
            BindingContext = new EditUserPageViewModel(user);
        }
    }
}
