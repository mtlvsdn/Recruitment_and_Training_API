using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;
using Microsoft.Maui.Controls;

namespace MauiClientApp.ViewModels
{
    public class UserTestsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly SessionManager _sessionManager;
        private ObservableCollection<Test> _tests;
        private bool _isLoading;
        private bool _hasTests;
        private string _statusMessage;

        public ObservableCollection<Test> Tests
        {
            get => _tests;
            set
            {
                if (_tests != value)
                {
                    _tests = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                if (_isLoading != value)
                {
                    _isLoading = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool HasTests
        {
            get => _hasTests;
            set
            {
                if (_hasTests != value)
                {
                    _hasTests = value;
                    OnPropertyChanged();
                }
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set
            {
                if (_statusMessage != value)
                {
                    _statusMessage = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand StartTestCommand { get; }
        public ICommand RefreshCommand { get; }

        public UserTestsViewModel()
        {
            _apiService = new ApiService();
            _sessionManager = SessionManager.Instance;
            Tests = new ObservableCollection<Test>();
            StartTestCommand = new Command<Test>(async (test) => await StartTestAsync(test));
            RefreshCommand = new Command(async () => await LoadUserTestsAsync());
            
            Console.WriteLine("UserTestsViewModel: Constructor called");
            // Don't load tests automatically - let the page handle this
            // to avoid duplicate loading
        }

        private async Task StartTestAsync(Test test)
        {
            try
            {
                if (test == null)
                {
                    Console.WriteLine("UserTestsViewModel: Error - test object is null");
                    await Application.Current.MainPage.DisplayAlert("Error", "Cannot start test: invalid test selected", "OK");
                    return;
                }

                // Debug output to check test details
                Console.WriteLine($"UserTestsViewModel: Starting test with ID: {test.test_id}");
                Console.WriteLine($"UserTestsViewModel: Test name: {test.test_name}");
                Console.WriteLine($"UserTestsViewModel: No. of questions: {test.no_of_questions}");
                Console.WriteLine($"UserTestsViewModel: Time limit: {test.time_limit}");
                Console.WriteLine($"UserTestsViewModel: Company name: {test.company_name}");
                
                // Ensure test object has valid data
                if (test.test_id <= 0)
                {
                    Console.WriteLine("UserTestsViewModel: Error - test ID is invalid");
                    await Application.Current.MainPage.DisplayAlert("Error", "Cannot start test: test ID is invalid", "OK");
                    return;
                }
                
                // Create a clean, valid Test object to pass to the navigation
                var safeTest = new Test
                {
                    test_id = test.test_id,
                    test_name = string.IsNullOrEmpty(test.test_name) ? $"Test {test.test_id}" : test.test_name,
                    no_of_questions = test.no_of_questions > 0 ? test.no_of_questions : 1,
                    time_limit = test.time_limit > 0 ? test.time_limit : 10,
                    company_name = string.IsNullOrEmpty(test.company_name) ? "Default Company" : test.company_name
                };
                
                try
                {
                    // Navigate to the test taking page with the test ID
                    var navigationParameter = new Dictionary<string, object>
                    {
                        { "TestId", safeTest.test_id.ToString() }
                    };
                    
                    Console.WriteLine($"UserTestsViewModel: Navigating to TestTakingPage with TestId={safeTest.test_id}");
                    
                    // Ensure we're on the main thread
                    await MainThread.InvokeOnMainThreadAsync(async () => {
                        try {
                            await Shell.Current.GoToAsync("/TestTakingPage", navigationParameter);
                            Console.WriteLine("UserTestsViewModel: Navigation command executed");
                        }
                        catch (Exception ex) {
                            Console.WriteLine($"UserTestsViewModel: Error during GoToAsync: {ex.Message}");
                            Console.WriteLine($"UserTestsViewModel: Stack trace: {ex.StackTrace}");
                            await Application.Current.MainPage.DisplayAlert("Navigation Error", $"Failed to navigate to test: {ex.Message}", "OK");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UserTestsViewModel: Error during navigation: {ex.Message}");
                    Console.WriteLine($"UserTestsViewModel: Stack trace: {ex.StackTrace}");
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to navigate to test: {ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserTestsViewModel: Error starting test: {ex.Message}");
                Console.WriteLine($"UserTestsViewModel: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to start test: {ex.Message}", "OK");
            }
        }

        public async Task LoadUserTestsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading your tests...";
                
                // Clear the existing tests collection
                Tests.Clear();
                
                // Get the current user's ID from session
                int userId = _sessionManager.UserId;
                if (userId <= 0)
                {
                    StatusMessage = "Error: User ID not found in session";
                    return;
                }

                Console.WriteLine($"UserTestsViewModel: Loading tests for user ID: {userId}");

                try
                {
                    // Step 1: Get the user's assigned test IDs from the user-test relationship table
                    var userTests = await _apiService.GetListAsync<UserTest>($"user-test/user/{userId}");
                    if (userTests == null || !userTests.Any())
                    {
                        StatusMessage = "You don't have any tests assigned to you.";
                        return;
                    }

                    Console.WriteLine($"UserTestsViewModel: Found {userTests.Count} user-test assignments");
                    
                    // Step 2: Get the list of ALL available tests
                    var allTests = await _apiService.GetListAsync<Test>("test");
                    if (allTests == null)
                    {
                        StatusMessage = "Error loading tests";
                        return;
                    }
                    
                    Console.WriteLine($"UserTestsViewModel: Retrieved {allTests.Count} total tests");
                    
                    // Step 3: CRITICAL FIX - Build a HashSet of test IDs we want to display
                    var assignedTestIds = new HashSet<int>(userTests.Select(ut => ut.Testtest_id).Distinct());
                    Console.WriteLine($"UserTestsViewModel: Found {assignedTestIds.Count} distinct test IDs");
                    
                    // Step 4: Create a completely new list of tests to display
                    var testsToDisplay = new List<Test>();
                    
                    // Step 5: For each test ID, find the best matching test from the list
                    foreach (var testId in assignedTestIds)
                    {
                        // Get all tests with this ID (should be only one, but we're being defensive)
                        var matchingTests = allTests.Where(t => t.test_id == testId).ToList();
                        if (matchingTests.Any())
                        {
                            // Take only the first one
                            var testToAdd = matchingTests.First();
                            Console.WriteLine($"UserTestsViewModel: Adding test ID {testToAdd.test_id}: {testToAdd.test_name}");
                            testsToDisplay.Add(testToAdd);
                        }
                        else
                        {
                            Console.WriteLine($"UserTestsViewModel: Could not find test with ID {testId}");
                        }
                    }
                    
                    // Step 6: Sort tests by ID to maintain a consistent order
                    var sortedTests = testsToDisplay.OrderBy(t => t.test_id).ToList();
                    
                    // Step 7: Rebuild the observable collection with the sorted, deduplicated tests
                    foreach (var test in sortedTests)
                    {
                        Tests.Add(test);
                    }
                    
                    // Update UI state
                    HasTests = Tests.Any();
                    StatusMessage = HasTests ? "" : "No tests found. Please contact your administrator.";
                    
                    Console.WriteLine($"UserTestsViewModel: Finished loading tests. Final count: {Tests.Count}");
                }
                catch (Exception ex)
                {
                    StatusMessage = "Error loading tests";
                    Console.WriteLine($"UserTestsViewModel: Error in LoadUserTestsAsync inner try: {ex.Message}");
                    Console.WriteLine($"UserTestsViewModel: Stack trace: {ex.StackTrace}");
                }
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error: {ex.Message}";
                Console.WriteLine($"UserTestsViewModel: Error in LoadUserTestsAsync: {ex.Message}");
                Console.WriteLine($"UserTestsViewModel: Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Alternative method to start test by ID, to avoid binding issues
        public async Task StartTestByIdAsync(int testId)
        {
            try
            {
                Console.WriteLine($"UserTestsViewModel: Starting test with ID: {testId} (direct method)");
                
                if (testId <= 0)
                {
                    Console.WriteLine("UserTestsViewModel: Invalid test ID");
                    await Application.Current.MainPage.DisplayAlert("Error", "Cannot start test: invalid test ID", "OK");
                    return;
                }
                
                // Get the test data to ensure it exists
                var test = Tests.FirstOrDefault(t => t.test_id == testId);
                Console.WriteLine($"UserTestsViewModel: Test found in local collection: {test != null}");
                
                if (test == null)
                {
                    Console.WriteLine($"UserTestsViewModel: Test with ID {testId} not found in local collection. Trying API...");
                    // Try to get it from the API as a fallback
                    try
                    {
                        test = await _apiService.GetAsync<Test>($"test/{testId}");
                        Console.WriteLine($"UserTestsViewModel: Test retrieved from API: {test != null}");
                        
                        if (test != null)
                        {
                            Console.WriteLine($"UserTestsViewModel: API test details - Name: {test.test_name}, Questions: {test.no_of_questions}, Time: {test.time_limit}");
                        }
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"UserTestsViewModel: Error getting test from API: {ex.Message}");
                        Console.WriteLine($"UserTestsViewModel: Stack trace: {ex.StackTrace}");
                    }
                    
                    if (test == null)
                    {
                        Console.WriteLine("UserTestsViewModel: Test not found in local collection or API");
                        await Application.Current.MainPage.DisplayAlert("Error", "Cannot start test: test not found", "OK");
                        return;
                    }
                }
                
                // Detailed logging of test object before creating safe copy
                Console.WriteLine($"UserTestsViewModel: Original test details - ID: {test.test_id}, Name: {test.test_name ?? "null"}, " +
                    $"Questions: {test.no_of_questions}, Time: {test.time_limit}, Company: {test.company_name ?? "null"}");
                
                // Create a clean, valid Test object to pass to the navigation
                var safeTest = new Test
                {
                    test_id = test.test_id,
                    test_name = string.IsNullOrEmpty(test.test_name) ? $"Test {test.test_id}" : test.test_name,
                    no_of_questions = test.no_of_questions > 0 ? test.no_of_questions : 1,
                    time_limit = test.time_limit > 0 ? test.time_limit : 10,
                    company_name = string.IsNullOrEmpty(test.company_name) ? "Default Company" : test.company_name
                };
                
                Console.WriteLine($"UserTestsViewModel: Safe test details - ID: {safeTest.test_id}, Name: {safeTest.test_name}, " +
                    $"Questions: {safeTest.no_of_questions}, Time: {safeTest.time_limit}, Company: {safeTest.company_name}");
                
                try
                {
                    // Navigate to the test taking page with the test ID
                    var navigationParameter = new Dictionary<string, object>
                    {
                        { "TestId", safeTest.test_id.ToString() }
                    };
                    
                    Console.WriteLine($"UserTestsViewModel: Navigating to TestTakingPage with TestId={safeTest.test_id}");
                    
                    // Ensure we're on the main thread
                    await MainThread.InvokeOnMainThreadAsync(async () => {
                        try {
                            await Shell.Current.GoToAsync("/TestTakingPage", navigationParameter);
                            Console.WriteLine("UserTestsViewModel: Navigation command executed");
                        }
                        catch (Exception ex) {
                            Console.WriteLine($"UserTestsViewModel: Error during GoToAsync: {ex.Message}");
                            Console.WriteLine($"UserTestsViewModel: Stack trace: {ex.StackTrace}");
                            await Application.Current.MainPage.DisplayAlert("Navigation Error", $"Failed to navigate to test: {ex.Message}", "OK");
                        }
                    });
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"UserTestsViewModel: Error during navigation: {ex.Message}");
                    Console.WriteLine($"UserTestsViewModel: Stack trace: {ex.StackTrace}");
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to navigate to test: {ex.Message}", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserTestsViewModel: Error starting test by ID: {ex.Message}");
                Console.WriteLine($"UserTestsViewModel: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to start test: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 