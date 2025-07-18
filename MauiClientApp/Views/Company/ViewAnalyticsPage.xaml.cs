using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.Company
{
    public partial class ViewAnalyticsPage : ContentPage
    {
        private TestAnalyticsViewModel _viewModel;
        
        public ViewAnalyticsPage()
        {
            InitializeComponent();
            _viewModel = new TestAnalyticsViewModel();
            BindingContext = _viewModel;
        }
        
        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Load tests when the page appears
            _viewModel.LoadTestsCommand.Execute(null);
        }
    }
} 