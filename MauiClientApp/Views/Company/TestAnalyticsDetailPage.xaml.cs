using MauiClientApp.ViewModels;
using MauiClientApp.Services;

namespace MauiClientApp.Views.Company
{
    public partial class TestAnalyticsDetailPage : ContentPage
    {
        private readonly TestAnalyticsDetailViewModel _viewModel;

        public TestAnalyticsDetailPage(int testId)
        {
            try
            {
                Console.WriteLine($"TestAnalyticsDetailPage: Constructor called with testId={testId}");
                
                InitializeComponent();
                
                Console.WriteLine("TestAnalyticsDetailPage: Components initialized");
                
                // Create ViewModel through DI
                _viewModel = new TestAnalyticsDetailViewModel(testId);
                
                if (_viewModel == null)
                {
                    Console.WriteLine("TestAnalyticsDetailPage: ERROR - _viewModel is null after creation");
                    throw new InvalidOperationException("Failed to create TestAnalyticsDetailViewModel.");
                }
                
                Console.WriteLine("TestAnalyticsDetailPage: View model successfully created");
                
                // Set binding context
                BindingContext = _viewModel;
                Console.WriteLine("TestAnalyticsDetailPage: BindingContext set");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestAnalyticsDetailPage: Error in constructor: {ex.Message}");
                Console.WriteLine($"TestAnalyticsDetailPage: Stack trace: {ex.StackTrace}");
                
                // Display an error to the user if the initialization fails
                MainThread.BeginInvokeOnMainThread(async () => 
                {
                    await DisplayAlert("Error", $"Failed to load analytics: {ex.Message}", "OK");
                });
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                Console.WriteLine("TestAnalyticsDetailPage: OnAppearing called");
                base.OnAppearing();
                
                // Check if the view model is null
                if (_viewModel == null)
                {
                    Console.WriteLine("TestAnalyticsDetailPage: ERROR - _viewModel is null in OnAppearing");
                    return;
                }
                
                // Check if the command is null
                if (_viewModel.LoadResultsCommand == null)
                {
                    Console.WriteLine("TestAnalyticsDetailPage: ERROR - LoadResultsCommand is null in OnAppearing");
                    return;
                }
                
                Console.WriteLine("TestAnalyticsDetailPage: Executing LoadResultsCommand");
                _viewModel.LoadResultsCommand.Execute(null);
                Console.WriteLine("TestAnalyticsDetailPage: LoadResultsCommand executed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestAnalyticsDetailPage: Error in OnAppearing: {ex.Message}");
                Console.WriteLine($"TestAnalyticsDetailPage: Stack trace: {ex.StackTrace}");
            }
        }
    }
} 