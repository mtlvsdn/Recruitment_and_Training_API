using MauiClientApp.Views;
using MauiClientApp.Views.Company;
using MauiClientApp.Views.User;

namespace MauiClientApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute(nameof(CompanyLoginPage), typeof(CompanyLoginPage));
            Routing.RegisterRoute(nameof(UserLoginPage), typeof(UserLoginPage));
            Routing.RegisterRoute(nameof(DashboardPage), typeof(DashboardPage));
            Routing.RegisterRoute(nameof(CompanyHomePage), typeof(CompanyHomePage));
        }
    }
}