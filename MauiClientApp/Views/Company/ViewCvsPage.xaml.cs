using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.Company
{
    public partial class ViewCvsPage : ContentPage
    {
        private ViewCvsViewModel _viewModel;
        
        public ViewCvsPage()
        {
            InitializeComponent();
            _viewModel = new ViewCvsViewModel();
            BindingContext = _viewModel;
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Load users when the page appears
            _viewModel.LoadUsersCommand.Execute(null);
        }
    }
} 