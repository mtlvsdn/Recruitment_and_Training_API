using MauiClientApp.Models;
using MauiClientApp.ViewModels;
using MauiClientApp.Services;
using System.ComponentModel;
using System.Linq;

namespace MauiClientApp.Views.Tests
{
    [QueryProperty(nameof(TestId), "TestId")]
    [QueryProperty(nameof(DebugTestId), "DebugTestId")]
    public partial class TestTakingPage : ContentPage, INotifyPropertyChanged
    {
        private TestTakingViewModel _viewModel;
        private string _testId;
        private Test _testData;
        private TestSession _testSession;
        private bool _isInitialized = false;
        private bool _isLoading = false;

        public event PropertyChangedEventHandler PropertyChanged;

        // Additional property for debugging
        public string DebugTestId
        {
            get => _testId;
            set
            {
                Console.WriteLine($"TestTakingPage: DebugTestId set to {value ?? "null"}");
                if (!string.IsNullOrEmpty(value) && string.IsNullOrEmpty(_testId))
                {
                    TestId = value; // Set the actual TestId if it's not already set
                }
            }
        }

        public string TestId
        {
            get => _testId;
            set
            {
                if (_testId == value) return;
                
                _testId = value;
                Console.WriteLine($"TestTakingPage: TestId set to {value ?? "null"}");
                
                if (value == null)
                {
                    Console.WriteLine("TestTakingPage: WARNING - TestId set to null");
                }
                
                // Only load test data after the page is fully initialized
                if (_isInitialized && !string.IsNullOrEmpty(_testId))
                {
                    Console.WriteLine("TestTakingPage: Page initialized and TestId not empty, calling LoadTestData()");
                    LoadTestData();
                }
                else
                {
                    if (!_isInitialized)
                        Console.WriteLine("TestTakingPage: Not calling LoadTestData because page is not initialized yet");
                    if (string.IsNullOrEmpty(_testId))
                        Console.WriteLine("TestTakingPage: Not calling LoadTestData because TestId is null or empty");
                }
            }
        }
        
        public TestSession TestSession
        {
            get => _testSession;
            private set
            {
                _testSession = value;
                // When set externally, update navigation to results page
                if (_testSession != null && _testSession.IsCompleted)
                {
                    NavigateToResults();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            private set
            {
                _isLoading = value;
                OnPropertyChanged(nameof(IsLoading));
            }
        }

        public TestTakingPage()
        {
            try
            {
                Console.WriteLine("TestTakingPage: Constructor called");
                InitializeComponent();
                
                // Ensure we have a ViewModel
                _viewModel = new TestTakingViewModel();
                Console.WriteLine("TestTakingPage: Created new TestTakingViewModel");
                
                // Set the binding context
                BindingContext = _viewModel;
                Console.WriteLine("TestTakingPage: Set BindingContext to TestTakingViewModel");
                
                // Initialize Test property with a default instance to avoid null reference
                _testData = new Test();
                Console.WriteLine("TestTakingPage: Created default Test instance to avoid null references");
                
                _isInitialized = true;
                Console.WriteLine("TestTakingPage: Page initialized");
                
                // Do NOT load test data in constructor - wait for OnAppearing
                // This prevents issues with partially initialized pages
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingPage: Error in constructor: {ex.Message}");
                Console.WriteLine($"TestTakingPage: Stack trace: {ex.StackTrace}");
                
                // Ensure we at least have a ViewModel to avoid further null references
                if (_viewModel == null)
                {
                    _viewModel = new TestTakingViewModel();
                    BindingContext = _viewModel;
                    Console.WriteLine("TestTakingPage: Created fallback ViewModel after exception");
                }
                
                // Show error on next frame to avoid constructor exceptions
                MainThread.BeginInvokeOnMainThread(async () => {
                    await DisplayAlert("Error", $"Failed to initialize test page: {ex.Message}", "OK");
                    await Shell.Current.GoToAsync("..");
                });
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                Console.WriteLine("TestTakingPage: OnAppearing called");
                
                // Make sure the ViewModel is initialized
                if (_viewModel == null)
                {
                    Console.WriteLine("TestTakingPage: Creating new ViewModel in OnAppearing because it was null");
                    _viewModel = new TestTakingViewModel();
                    BindingContext = _viewModel;
                }
                
                // If we have the test data but haven't initialized the test session yet
                if (_testData != null && (_viewModel.TestSession == null || _viewModel.TestSession.Test == null))
                {
                    Console.WriteLine("TestTakingPage: Test data available but not loaded in ViewModel, initializing test");
                    InitializeTest();
                }
                // If we don't have test data but have a test ID, try to load it
                else if (_testData == null && !string.IsNullOrEmpty(_testId))
                {
                    Console.WriteLine($"TestTakingPage: No test data but have TestId {_testId}, loading data");
                    LoadTestData();
                }
                // If we have no test data and no test ID, show an error
                else if (_testData == null && string.IsNullOrEmpty(_testId))
                {
                    Console.WriteLine("TestTakingPage: No test data and no TestId, showing error");
                    MainThread.BeginInvokeOnMainThread(async () => {
                        await DisplayAlert("Error", "Failed to start test: No test data available", "OK");
                        await Shell.Current.GoToAsync("..");
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingPage: Error in OnAppearing: {ex.Message}");
                Console.WriteLine($"TestTakingPage: Stack trace: {ex.StackTrace}");
                MainThread.BeginInvokeOnMainThread(async () => {
                    await DisplayAlert("Error", $"Error initializing test: {ex.Message}", "OK");
                    await Shell.Current.GoToAsync("..");
                });
            }
        }
        
        protected override void OnDisappearing()
        {
            try
            {
                base.OnDisappearing();
                Console.WriteLine("TestTakingPage: OnDisappearing called");
                
                // Keep track of the test session if we're navigating away
                if (_viewModel?.TestSession != null)
                {
                    _testSession = _viewModel.TestSession;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingPage: Error in OnDisappearing: {ex.Message}");
            }
        }

        private async void LoadTestData()
        {
            try
            {
                Console.WriteLine("TestTakingPage: LoadTestData called");
                
                if (string.IsNullOrEmpty(_testId))
                {
                    Console.WriteLine("TestTakingPage: TestId is null or empty");
                    await DisplayAlert("Error", "Invalid test ID", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }

                if (!int.TryParse(_testId, out int testId))
                {
                    Console.WriteLine($"TestTakingPage: Failed to parse TestId '{_testId}' as integer");
                    
                    // Try to clean the test ID and retry
                    string cleanedId = new string(_testId.Where(c => char.IsDigit(c)).ToArray());
                    Console.WriteLine($"TestTakingPage: Cleaned TestId to '{cleanedId}'");
                    
                    if (!string.IsNullOrEmpty(cleanedId) && int.TryParse(cleanedId, out testId))
                    {
                        Console.WriteLine($"TestTakingPage: Successfully parsed cleaned TestId to {testId}");
                    }
                    else
                    {
                        Console.WriteLine("TestTakingPage: Even cleaned TestId couldn't be parsed");
                        await DisplayAlert("Error", "Invalid test ID format", "OK");
                        await Shell.Current.GoToAsync("..");
                        return;
                    }
                }

                Console.WriteLine($"TestTakingPage: Loading test data for ID: {testId}");
                IsLoading = true;
                
                try
                {
                    // Make sure the ApiService is created
                    var apiService = new MauiClientApp.Services.ApiService();
                    Console.WriteLine($"TestTakingPage: Created ApiService");
                    
                    // Get the test from API
                    Console.WriteLine($"TestTakingPage: Requesting test/{testId} from API");
                    _testData = await apiService.GetAsync<Test>($"test/{testId}");
                    
                    if (_testData == null)
                    {
                        Console.WriteLine("TestTakingPage: API returned null test data");
                        await DisplayAlert("Error", "Failed to load test data", "OK");
                        await Shell.Current.GoToAsync("..");
                        return;
                    }
                    
                    Console.WriteLine($"TestTakingPage: API returned test data - ID: {_testData.test_id}, Name: {_testData.test_name ?? "null"}");

                    // Create a new test object with the data to ensure all properties are properly initialized
                    var safeTestData = new Test
                    {
                        test_id = _testData.test_id,
                        test_name = _testData.test_name ?? $"Test {_testData.test_id}",
                        no_of_questions = _testData.no_of_questions > 0 ? _testData.no_of_questions : 1,
                        time_limit = _testData.time_limit > 0 ? _testData.time_limit : 10,
                        company_name = !string.IsNullOrEmpty(_testData.company_name) ? _testData.company_name : "Default Company"
                    };
                    
                    // Replace the original test data with the safe copy
                    _testData = safeTestData;
                    
                    Console.WriteLine($"TestTakingPage: Successfully loaded test: {_testData.test_name} (ID: {_testData.test_id})");
                    Console.WriteLine($"TestTakingPage: Questions: {_testData.no_of_questions}, Time Limit: {_testData.time_limit}");
                    
                    // Now check if there are questions for this test
                    Console.WriteLine($"TestTakingPage: Requesting questions/test/{testId} from API");
                    var questions = await apiService.GetListAsync<Question>($"questions/test/{testId}");
                    
                    if (questions == null || !questions.Any())
                    {
                        Console.WriteLine("TestTakingPage: No questions found for this test");
                        await DisplayAlert("Error", "This test does not have any questions", "OK");
                        await Shell.Current.GoToAsync("..");
                        return;
                    }
                    
                    Console.WriteLine($"TestTakingPage: Found {questions.Count} questions for the test");
                    
                    // Initialize the test session
                    InitializeTest();
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TestTakingPage: Error loading test data: {ex.Message}");
                    Console.WriteLine($"TestTakingPage: Stack trace: {ex.StackTrace}");
                    await DisplayAlert("Error", $"Failed to load test data: {ex.Message}", "OK");
                    await Shell.Current.GoToAsync("..");
                }
                finally
                {
                    IsLoading = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingPage: Unhandled error in LoadTestData: {ex.Message}");
                Console.WriteLine($"TestTakingPage: Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", $"Unhandled error: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }

        private async void InitializeTest()
        {
            try
            {
                Console.WriteLine("TestTakingPage: InitializeTest called");
                
                if (_testData == null)
                {
                    Console.WriteLine("TestTakingPage: No test data available");
                    await DisplayAlert("Error", "Cannot start test: No test data available", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }
                
                // Log test data we have so far
                Console.WriteLine($"TestTakingPage: Test data ID: {_testData.test_id}, Name: {_testData.test_name ?? "null"}, " +
                    $"Questions: {_testData.no_of_questions}, Time: {_testData.time_limit}, Company: {_testData.company_name ?? "null"}");
                
                // Ensure _viewModel is initialized
                if (_viewModel == null)
                {
                    Console.WriteLine("TestTakingPage: ViewModel was null, creating new instance");
                    _viewModel = new TestTakingViewModel();
                    BindingContext = _viewModel;
                }
                
                // Verify test data has required properties
                if (_testData.test_id <= 0)
                {
                    Console.WriteLine("TestTakingPage: Invalid test ID in test data");
                    await DisplayAlert("Error", "Invalid test data: Missing test ID", "OK");
                    await Shell.Current.GoToAsync("..");
                    return;
                }
                
                // If the test has no name, provide a default
                if (string.IsNullOrEmpty(_testData.test_name))
                {
                    _testData.test_name = $"Test {_testData.test_id}";
                    Console.WriteLine($"TestTakingPage: Test had no name, set to {_testData.test_name}");
                }
                
                // Ensure test has valid number of questions and time limit
                if (_testData.no_of_questions <= 0)
                {
                    _testData.no_of_questions = 1;
                    Console.WriteLine("TestTakingPage: Test had invalid question count, set to 1");
                }
                
                if (_testData.time_limit <= 0)
                {
                    _testData.time_limit = 10;
                    Console.WriteLine("TestTakingPage: Test had invalid time limit, set to 10 minutes");
                }
                
                // Ensure TestSession in ViewModel is initialized
                if (_viewModel.TestSession == null)
                {
                    Console.WriteLine("TestTakingPage: TestSession in ViewModel is null, creating a new one");
                    _viewModel.TestSession = new TestSession();
                }
                
                Console.WriteLine($"TestTakingPage: Starting test initialization for: {_testData.test_name} (ID: {_testData.test_id})");
                
                try
                {
                    // Initialize the test in the ViewModel
                    await _viewModel.InitializeTestAsync(_testData);
                    
                    // Check if initialization succeeded by validating TestSession
                    if (_viewModel.TestSession == null || _viewModel.TestSession.Test == null)
                    {
                        Console.WriteLine("TestTakingPage: Test session was not properly initialized");
                        await DisplayAlert("Error", "Failed to start test: Test session initialization error", "OK");
                        await Shell.Current.GoToAsync("..");
                        return;
                    }
                    
                    // Set up property change notification
                    _viewModel.PropertyChanged += OnViewModelPropertyChanged;
                    
                    Console.WriteLine("TestTakingPage: Test initialized successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TestTakingPage: Error in test initialization: {ex.Message}");
                    Console.WriteLine($"TestTakingPage: Stack trace: {ex.StackTrace}");
                    await DisplayAlert("Error", $"Failed to start test: {ex.Message}", "OK");
                    await Shell.Current.GoToAsync("..");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingPage: Unhandled error in InitializeTest: {ex.Message}");
                Console.WriteLine($"TestTakingPage: Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", $"Failed to start test: {ex.Message}", "OK");
                await Shell.Current.GoToAsync("..");
            }
        }
        
        private void OnViewModelPropertyChanged(object sender, System.ComponentModel.PropertyChangedEventArgs e)
        {
            try
            {
                if (e.PropertyName == nameof(TestTakingViewModel.TestSession))
                {
                    if (_viewModel.TestSession?.IsCompleted == true)
                    {
                        // Test completed, navigate to results
                        Console.WriteLine("TestTakingPage: Test completed, navigating to results");
                        _testSession = _viewModel.TestSession;
                        NavigateToResults();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingPage: Error in property changed handler: {ex.Message}");
            }
        }
        
        private async void NavigateToResults()
        {
            try
            {
                if (_testSession == null)
                {
                    Console.WriteLine("TestTakingPage: Cannot navigate to results - TestSession is null");
                    await DisplayAlert("Error", "Test results are not available", "OK");
                    await Shell.Current.GoToAsync("//dashboard");
                    return;
                }
                
                Console.WriteLine("TestTakingPage: Navigating to results page");
                
                // Navigate to results page
                var resultsPage = new TestResultsPage(_testSession);
                await Navigation.PushAsync(resultsPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingPage: Error navigating to results: {ex.Message}");
                Console.WriteLine($"TestTakingPage: Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", "Failed to show test results", "OK");
                await Shell.Current.GoToAsync("//dashboard");
            }
        }

        private void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
        }
    }
} 