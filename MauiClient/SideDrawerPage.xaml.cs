namespace MauiClient
{
    public partial class SideDrawerPage : ContentPage
    {
        public SideDrawerPage()
        {
            InitializeComponent();
        }

        private async void OnAddCompanyClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("AddCompanyPage");
        }

        private async void OnLogoutButtonClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync("//LoginPage");
        }
    }
}
