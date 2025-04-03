using MauiClientApp.ViewModels;
using Microsoft.Maui.Controls;

namespace MauiClientApp.Views.Company
{
    public partial class CreateUserPage : ContentPage
    {
        public CreateUserPage()
        {
            InitializeComponent();
            BindingContext = new CreateUserPageViewModel();
        }
    }
}
