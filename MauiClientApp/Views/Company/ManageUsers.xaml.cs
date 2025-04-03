using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.Company
{
    public partial class ManageUsers : ContentPage
    {
        private readonly ManageUsersViewModel _viewModel;
        public ManageUsers()
        {
            InitializeComponent();
            _viewModel = new ManageUsersViewModel();
            BindingContext = _viewModel;
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            _viewModel.LoadUsersAsync();
        }
    }
}
