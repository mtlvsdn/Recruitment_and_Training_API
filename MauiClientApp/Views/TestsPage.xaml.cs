using MauiClientApp.ViewModels;

namespace MauiClientApp.Views
{
    public partial class TestsPage : ContentPage
    {
        private TestsViewModel _viewModel;

        public TestsPage()
        {
            InitializeComponent();
            _viewModel = new TestsViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            await _viewModel.LoadTestsAsync();
        }
    }
} 