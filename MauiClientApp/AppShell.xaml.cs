using MauiClientApp.Views;
using MauiClientApp.Views.Company;
using MauiClientApp.Views.User;
using MauiClientApp.Views.Tests;

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
            Routing.RegisterRoute("dashboard", typeof(DashboardPage));
            Routing.RegisterRoute("manageusers", typeof(MauiClientApp.Views.Company.ManageUsers));
            Routing.RegisterRoute("createuser", typeof(MauiClientApp.Views.Company.CreateUserPage));
            Routing.RegisterRoute(nameof(TestsPage), typeof(TestsPage));
            Routing.RegisterRoute("TestSummaryPage", typeof(Views.Tests.TestSummaryPage));
            Routing.RegisterRoute("AssignUsersPage", typeof(Views.Tests.AssignUsersPage));
            Routing.RegisterRoute("EditTestPage", typeof(MauiClientApp.Views.Tests.EditTestPage));
        }
    }
}