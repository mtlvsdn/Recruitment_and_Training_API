namespace MauiClient
{
    public partial class AppShell : Shell
    {
        public AppShell()
        {
            InitializeComponent();

            Routing.RegisterRoute("HomePage", typeof(HomePage));
            Routing.RegisterRoute("LoginPage", typeof(LoginPage));
            Routing.RegisterRoute(nameof(AddCompanyPage), typeof(AddCompanyPage));
            Routing.RegisterRoute(nameof(NewCompanyPage), typeof(NewCompanyPage));


            // Listen for navigation changes
            Navigated += OnNavigated;
        }

        private void OnNavigated(object sender, ShellNavigatedEventArgs e)
        {
            // Check if current page is LoginPage and disable the drawer
            if (CurrentPage is LoginPage)
            {
                this.FlyoutBehavior = FlyoutBehavior.Disabled;
            }
            else
            {
                this.FlyoutBehavior = FlyoutBehavior.Flyout;
            }
        }

        private async void OnAddCompanyClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(AddCompanyPage));
        }

        private async void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }

        private async void OnNewCompanyClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync(nameof(NewCompanyPage));
        }
    }
}
