using MauiClientApp.Models;
using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.Tests
{
    [QueryProperty(nameof(TestId), "TestId")]
    public partial class EditTestPage : ContentPage, IQueryAttributable
    {
        private string _testId;
        private EditTestViewModel _viewModel;
        private bool _isInitialized = false;
        private bool _isLoadingData = false;

        public string TestId
        {
            get => _testId;
            set
            {
                if (_testId == value)
                    return;
                
                Console.WriteLine($"EditTestPage: TestId property set to {value}");
                _testId = value;
                
                // Only proceed with loading if we're fully initialized
                if (_isInitialized && _viewModel != null && !_isLoadingData)
                {
                    Console.WriteLine("EditTestPage: Calling LoadTestData() after TestId set");
                    LoadTestData();
                }
                else
                {
                    Console.WriteLine("EditTestPage: Delaying LoadTestData until initialization is complete");
                }
            }
        }

        private async void LoadTestData()
        {
            try
            {
                if (_viewModel == null)
                {
                    Console.WriteLine("EditTestPage: _viewModel is null, cannot load test data");
                    await Application.Current.MainPage.DisplayAlert("Error", "ViewModel not initialized properly", "OK");
                    return;
                }

                if (string.IsNullOrEmpty(_testId))
                {
                    Console.WriteLine("EditTestPage: _testId is null or empty, cannot load test data");
                    await Application.Current.MainPage.DisplayAlert("Error", "Test ID not provided", "OK");
                    return;
                }

                if (!int.TryParse(_testId, out int id))
                {
                    Console.WriteLine($"EditTestPage: Could not parse _testId: {_testId} as integer");
                    await Application.Current.MainPage.DisplayAlert("Error", $"Invalid test ID format: {_testId}", "OK");
                    return;
                }

                Console.WriteLine($"EditTestPage: Setting Test in ViewModel with ID: {id}");
                
                _isLoadingData = true;
                
                // Create a new Test with the ID and assign it to the ViewModel
                var test = new Test { test_id = id };
                _viewModel.Test = test;
                
                // Wait a short time to ensure the load operation has started
                await Task.Delay(100);
                _isLoadingData = false;
            }
            catch (Exception ex)
            {
                _isLoadingData = false;
                Console.WriteLine($"EditTestPage: Exception in LoadTestData: {ex.Message}");
                Console.WriteLine($"EditTestPage: StackTrace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Error loading test: {ex.Message}", "OK");
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            try
            {
                Console.WriteLine($"EditTestPage: ApplyQueryAttributes called with {query.Count} parameters");
                
                if (query.TryGetValue("TestId", out var testIdObj))
                {
                    Console.WriteLine($"EditTestPage: Found TestId in query: {testIdObj}");
                    
                    // Handle different types that could be passed
                    if (testIdObj is string testIdStr)
                    {
                        TestId = testIdStr;
                    }
                    else if (testIdObj is int testIdInt)
                    {
                        TestId = testIdInt.ToString();
                    }
                    else
                    {
                        Console.WriteLine($"EditTestPage: TestId is of unexpected type: {testIdObj?.GetType().Name ?? "null"}");
                        Application.Current.MainPage.DisplayAlert("Error", "Invalid test ID type", "OK");
                    }
                }
                else
                {
                    Console.WriteLine("EditTestPage: TestId not found in query parameters");
                    Application.Current.MainPage.DisplayAlert("Error", "Test ID not found in navigation parameters", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditTestPage: Exception in ApplyQueryAttributes: {ex.Message}");
                Console.WriteLine($"EditTestPage: StackTrace: {ex.StackTrace}");
                Application.Current.MainPage.DisplayAlert("Error", $"Error applying query attributes: {ex.Message}", "OK");
            }
        }

        public EditTestPage()
        {
            try
            {
                Console.WriteLine("EditTestPage: Constructor called");
                InitializeComponent();
                
                Console.WriteLine("EditTestPage: Creating ViewModel");
                _viewModel = new EditTestViewModel();
                BindingContext = _viewModel;
                
                // Mark as initialized after everything is set up
                _isInitialized = true;
                Console.WriteLine("EditTestPage: Page initialization completed, _isInitialized = true");
                
                // If TestId was set before initialization completed, load the data now
                if (!string.IsNullOrEmpty(_testId))
                {
                    Console.WriteLine($"EditTestPage: TestId was set before initialization to {_testId}, loading now");
                    LoadTestData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditTestPage: Exception during initialization: {ex.Message}");
                Console.WriteLine($"EditTestPage: StackTrace: {ex.StackTrace}");
                Application.Current.MainPage.DisplayAlert("Error", $"Error initializing page: {ex.Message}", "OK");
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                Console.WriteLine("EditTestPage: OnAppearing called");
                
                // Re-check if we need to load data here
                if (_viewModel != null && !_isLoadingData && 
                    (_viewModel.Test == null || _viewModel.Test.test_id <= 0 || string.IsNullOrEmpty(_viewModel.TestTitle)))
                {
                    Console.WriteLine("EditTestPage: Test data missing, reloading data in OnAppearing");
                    LoadTestData();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditTestPage: Exception in OnAppearing: {ex.Message}");
                Console.WriteLine($"EditTestPage: StackTrace: {ex.StackTrace}");
                Application.Current.MainPage.DisplayAlert("Error", $"Error on page appearance: {ex.Message}", "OK");
            }
        }
    }
} 