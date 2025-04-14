using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;

namespace MauiClientApp.ViewModels
{
    public class TestsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private ObservableCollection<Test> _tests;
        private bool _isLoading;
        private bool _hasTests;
        private bool _showNoTestsMessage;

        public ICommand EditTestCommand { get; }
        public ICommand AssignTestCommand { get; }
        public ICommand DeleteTestCommand { get; }

        public ObservableCollection<Test> Tests
        {
            get => _tests;
            set
            {
                _tests = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public bool HasTests
        {
            get => _hasTests;
            set
            {
                _hasTests = value;
                OnPropertyChanged();
            }
        }

        public bool ShowNoTestsMessage
        {
            get => _showNoTestsMessage;
            set
            {
                _showNoTestsMessage = value;
                OnPropertyChanged();
            }
        }

        public TestsViewModel()
        {
            _apiService = new ApiService();
            Tests = new ObservableCollection<Test>();

            EditTestCommand = new Command<Test>(async (test) => await OnEditTest(test));
            AssignTestCommand = new Command<Test>(async (test) => await OnAssignTest(test));
            DeleteTestCommand = new Command<Test>(async (test) => await OnDeleteTest(test));
        }

        private async Task OnEditTest(Test test)
        {
            try
            {
                if (test == null)
                {
                    Console.WriteLine("OnEditTest: test parameter is null");
                    await Application.Current.MainPage.DisplayAlert("Error", "Cannot edit: Invalid test data", "OK");
                    return;
                }

                if (test.test_id <= 0)
                {
                    Console.WriteLine($"OnEditTest: test ID is invalid: {test.test_id}");
                    await Application.Current.MainPage.DisplayAlert("Error", "Cannot edit: Test ID is invalid", "OK");
                    return;
                }

                Console.WriteLine($"OnEditTest: Navigating to EditTestPage with test ID: {test.test_id}");
                
                try
                {
                    // Create a valid test ID parameter
                    string testIdStr = test.test_id.ToString();
                    Console.WriteLine($"OnEditTest: Test ID parameter: {testIdStr}");
                    
                    // Create navigation parameters
                    var navigationParameter = new Dictionary<string, object>
                    {
                        { "TestId", testIdStr }
                    };

                    // Try direct creation of the page first if GoToAsync fails
                    try
                    {
                        // Use ShellNavigationManager to navigate with parameters
                        await Shell.Current.GoToAsync("EditTestPage", navigationParameter);
                        Console.WriteLine("OnEditTest: Navigation completed successfully via Shell.GoToAsync");
                    }
                    catch (Exception shellEx)
                    {
                        // If Shell navigation fails, try direct page creation
                        Console.WriteLine($"OnEditTest: Shell navigation failed: {shellEx.Message}, trying direct page creation");
                        var page = new MauiClientApp.Views.Tests.EditTestPage();
                        ((IQueryAttributable)page).ApplyQueryAttributes(navigationParameter);
                        await Application.Current.MainPage.Navigation.PushAsync(page);
                        Console.WriteLine("OnEditTest: Navigation completed successfully via direct page creation");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OnEditTest: Inner exception during navigation: {ex.Message}");
                    Console.WriteLine($"OnEditTest: Inner StackTrace: {ex.StackTrace}");
                    throw; // Re-throw to be caught by the outer try-catch
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnEditTest: Exception during navigation: {ex.Message}");
                Console.WriteLine($"OnEditTest: StackTrace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to open edit page: {ex.Message}", "OK");
            }
        }

        private async Task OnAssignTest(Test test)
        {
            if (test == null)
            {
                Console.WriteLine("OnAssignTest: Received null test parameter");
                await Application.Current.MainPage.DisplayAlert("Error", "Invalid test data", "OK");
                return;
            }

            try
            {
                Console.WriteLine($"OnAssignTest: Navigating to AssignUsersPage with test: {test.test_name} (ID: {test.test_id})");
                
                // Ensure the test object has required collections initialized
                if (test.Questions == null)
                {
                    test.Questions = new ObservableCollection<Question>();
                }
                if (test.AssignedUsers == null)
                {
                    test.AssignedUsers = new ObservableCollection<User>();
                }

                // Create the page directly
                var page = new MauiClientApp.Views.Tests.AssignUsersPage();
                
                // Create a small delay to ensure the page is fully initialized
                await Task.Delay(50);
                
                // Set the Test property directly
                ((dynamic)page).Test = test;
                Console.WriteLine($"OnAssignTest: Test set to page, test ID: {test.test_id}");
                
                // Navigate using PushAsync
                await Application.Current.MainPage.Navigation.PushAsync(page);
                Console.WriteLine("OnAssignTest: Navigation completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"OnAssignTest: Navigation error: {ex.Message}");
                Console.WriteLine($"OnAssignTest: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to open assignment page", "OK");
            }
        }

        private async Task OnDeleteTest(Test test)
        {
            if (test == null) return;

            // Ask for confirmation
            bool answer = await Application.Current.MainPage.DisplayAlert(
                "Confirm Delete",
                $"Are you sure you want to delete the test '{test.test_name}'? This will also delete all associated questions and user assignments.",
                "Yes", "No");

            if (answer)
            {
                try
                {
                    Console.WriteLine($"TestsViewModel: Attempting to delete test with ID: {test.test_id}");
                    
                    // The API will handle deleting associated questions and user-test associations
                    bool isDeleted = await _apiService.DeleteAsync($"test/{test.test_id}");
                    Console.WriteLine($"TestsViewModel: Delete API call completed. Result: {isDeleted}");
                    
                    if (isDeleted)
                    {
                        // Remove from collection only if the API call was successful
                        Tests.Remove(test);
                        
                        // Update UI state
                        HasTests = Tests.Any();
                        ShowNoTestsMessage = !HasTests;
                        
                        await Application.Current.MainPage.DisplayAlert(
                            "Success", 
                            "Test and all associated data deleted successfully", 
                            "OK");
                    }
                    else
                    {
                        throw new Exception("Failed to delete test from the database.");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"TestsViewModel: Error deleting test: {ex.Message}");
                    Console.WriteLine($"TestsViewModel: Stack trace: {ex.StackTrace}");
                    
                    await Application.Current.MainPage.DisplayAlert(
                        "Error",
                        $"Failed to delete test: {ex.Message}",
                        "OK");
                    
                    // Refresh the tests list to ensure UI is in sync with database
                    await LoadTestsAsync();
                }
            }
        }

        public async Task LoadTestsAsync()
        {
            try
            {
                IsLoading = true;
                ShowNoTestsMessage = false;
                HasTests = false;

                // Get company name from session
                var companyName = SessionManager.Instance?.CompanyName;
                if (string.IsNullOrEmpty(companyName))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Company name not found in session", "OK");
                    return;
                }

                Console.WriteLine($"LoadTestsAsync: Loading tests for company: {companyName}");

                // Clear existing tests
                Tests.Clear();
                Console.WriteLine("LoadTestsAsync: Cleared existing tests collection");

                // Get all tests
                var allTests = await _apiService.GetListAsync<Test>("test");
                Console.WriteLine($"LoadTestsAsync: Retrieved {allTests?.Count ?? 0} tests from API");

                if (allTests != null)
                {
                    // Filter tests by company name and make sure we have unique test_ids
                    var companyTests = allTests
                        .Where(t => t.company_name == companyName)
                        .GroupBy(t => t.test_id) // Group by test_id to eliminate duplicates
                        .Select(g => g.First())  // Take the first test from each group
                        .ToList();

                    Console.WriteLine($"LoadTestsAsync: Filtered to {companyTests.Count} unique tests for company {companyName}");

                    if (companyTests.Any())
                    {
                        foreach (var test in companyTests)
                        {
                            Console.WriteLine($"LoadTestsAsync: Adding test: {test.test_name} (ID: {test.test_id})");
                            Tests.Add(test);
                        }
                        HasTests = true;
                        ShowNoTestsMessage = false;
                    }
                    else
                    {
                        Console.WriteLine("LoadTestsAsync: No tests found for company");
                        HasTests = false;
                        ShowNoTestsMessage = true;
                    }
                }
                else
                {
                    Console.WriteLine("LoadTestsAsync: Failed to retrieve tests from API");
                    HasTests = false;
                    ShowNoTestsMessage = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"LoadTestsAsync: Error loading tests: {ex.Message}");
                Console.WriteLine($"LoadTestsAsync: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load tests: {ex.Message}", "OK");
                HasTests = false;
                ShowNoTestsMessage = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 