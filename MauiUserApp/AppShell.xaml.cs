using MauiUserApp.Views;

namespace MauiUserApp
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("StartUpPage", typeof(StartUpPage));
            Routing.RegisterRoute("CompanyLoginPage", typeof(CompLoginPage));
            Routing.RegisterRoute("UserLoginPage", typeof(UserLoginPage));

            //Routing.RegisterRoute(nameof(AddCompanyPage), typeof(AddCompanyPage));
            //Routing.RegisterRoute(nameof(NewCompanyPage), typeof(NewCompanyPage));


            // Listen for navigation changes
            Navigated += OnNavigated;
        }

        private void OnNavigated(object sender, ShellNavigatedEventArgs e)
        {
            // Check if current page is LoginPage and disable the drawer
            if (CurrentPage is StartUpPage || CurrentPage is CompLoginPage || CurrentPage is UserLoginPage)
            {
                this.FlyoutBehavior = FlyoutBehavior.Disabled;
            }
            else
            {
                this.FlyoutBehavior = FlyoutBehavior.Flyout;
            }
        }

        //private async void OnAddCompanyClicked(object sender, EventArgs e)
        //{
        //    await Shell.Current.GoToAsync(nameof(AddCompanyPage));
        //}

        //private async void OnLogoutButtonClicked(object sender, EventArgs e)
        //{
        //    await Shell.Current.GoToAsync("//LoginPage");
        //}

        //private async void OnNewCompanyClicked(object sender, EventArgs e)
        //{
        //    await Shell.Current.GoToAsync(nameof(NewCompanyPage));
        //}
    }
}
