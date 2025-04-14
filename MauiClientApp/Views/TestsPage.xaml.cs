using MauiClientApp.ViewModels;

namespace MauiClientApp.Views
{
    public partial class TestsPage : ContentPage
    {
        private TestsViewModel _viewModel;
        private bool _hasAppeared = false;

        public TestsPage()
        {
            InitializeComponent();
            Console.WriteLine("TestsPage: Constructor called");
            _viewModel = new TestsViewModel();
            BindingContext = _viewModel;
        }

        protected override async void OnAppearing()
        {
            base.OnAppearing();
            
            Console.WriteLine("TestsPage: OnAppearing called");
            
            // Only load tests if this is the first time the page appears or after a navigation
            if (!_hasAppeared)
            {
                Console.WriteLine("TestsPage: Loading tests for the first time");
                _hasAppeared = true;
                await _viewModel.LoadTestsAsync();
            }
            else
            {
                Console.WriteLine("TestsPage: Tests already loaded, skipping reload");
            }
        }

        private async void RefreshButton_Clicked(object sender, EventArgs e)
        {
            Console.WriteLine("TestsPage: Refresh button clicked, forcing reload of tests");
            await _viewModel.LoadTestsAsync();
        }
    }
} 