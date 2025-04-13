using MauiClientApp.ViewModels;
using Microsoft.Maui.Controls;

namespace MauiClientApp.Views.Tests
{
    [QueryProperty(nameof(Test), "Test")]
    public partial class TestSummaryPage : ContentPage
    {
        private readonly TestSummaryViewModel _viewModel;

        public Models.Test Test
        {
            set
            {
                try
                {
                    Console.WriteLine($"TestSummaryPage: Setting Test property");
                    if (value != null)
                    {
                        Console.WriteLine($"TestSummaryPage: Test received - Name: {value.TestName}, Questions: {value.Questions?.Count ?? 0}");
                        _viewModel.Test = value;
                    }
                    else
                    {
                        Console.WriteLine("TestSummaryPage: Received null Test value");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TestSummaryPage: Error setting Test property: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        public TestSummaryPage()
        {
            try
            {
                Console.WriteLine("TestSummaryPage: Starting initialization");
                InitializeComponent();
                _viewModel = new TestSummaryViewModel();
                BindingContext = _viewModel;
                Console.WriteLine("TestSummaryPage: Initialization completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestSummaryPage: Error in constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                throw; // Re-throw to ensure the error is visible
            }
        }

        protected override async void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                Console.WriteLine("TestSummaryPage: OnAppearing called");
                
                if (_viewModel?.Test == null)
                {
                    Console.WriteLine("TestSummaryPage: No test data available");
                    await Shell.Current.GoToAsync("//TestsPage");
                    return;
                }
                
                Console.WriteLine($"TestSummaryPage: Test data available - Name: {_viewModel.Test.TestName}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestSummaryPage: Error in OnAppearing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", "Failed to load test data", "OK");
                await Shell.Current.GoToAsync("//TestsPage");
            }
        }
    }
} 