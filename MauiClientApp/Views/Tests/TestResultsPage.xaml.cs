using MauiClientApp.Models;
using MauiClientApp.ViewModels;
using MauiClientApp.Services;

namespace MauiClientApp.Views.Tests
{
    public partial class TestResultsPage : ContentPage
    {
        private TestResultsViewModel _viewModel;

        public TestResultsPage(TestSession testSession)
        {
            InitializeComponent();
            
            try
            {
                Console.WriteLine("TestResultsPage: Constructor called");
                
                if (testSession == null)
                {
                    Console.WriteLine("TestResultsPage: Warning - testSession is null");
                    DisplayAlert("Error", "Test results are not available", "OK");
                    return;
                }

                _viewModel = new TestResultsViewModel(testSession);
                BindingContext = _viewModel;
                
                Console.WriteLine("TestResultsPage: Page initialized successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestResultsPage: Error in constructor: {ex.Message}");
                Console.WriteLine($"TestResultsPage: Stack trace: {ex.StackTrace}");
                DisplayAlert("Error", "Failed to load test results", "OK");
            }
        }

        protected override bool OnBackButtonPressed()
        {
            // Override back button to use the same navigation as the Return to Dashboard button
            if (_viewModel != null)
            {
                _viewModel.ReturnToDashboardCommand.Execute(null);
            }
            return true;
        }
    }
} 