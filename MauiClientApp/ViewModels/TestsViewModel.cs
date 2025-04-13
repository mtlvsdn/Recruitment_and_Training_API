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
                    // Create the page and set the test ID
                    var page = new MauiClientApp.Views.Tests.EditTestPage();
                    
                    // Make sure the ViewModel is initialized before setting the TestId
                    await Task.Delay(50); // Brief delay to ensure the page is fully initialized
                    
                    // Set the test ID - this will trigger loading the test data
                    page.TestId = test.test_id.ToString();
                    Console.WriteLine($"OnEditTest: Set TestId to {test.test_id}, now navigating");
                    
                    // Navigate to the page
                    await Application.Current.MainPage.Navigation.PushAsync(page);
                    Console.WriteLine("OnEditTest: Navigation completed successfully");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"OnEditTest: Inner exception during page creation/navigation: {ex.Message}");
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

                Console.WriteLine($"Loading tests for company: {companyName}");

                // Clear existing tests
                Tests.Clear();

                // Get all tests
                var allTests = await _apiService.GetListAsync<Test>("test");

                if (allTests != null)
                {
                    // Filter tests by company name
                    var companyTests = allTests.Where(t => t.company_name == companyName).ToList();

                    if (companyTests.Any())
                    {
                        foreach (var test in companyTests)
                        {
                            Console.WriteLine($"Adding test: {test.test_name} (ID: {test.test_id})");
                            Tests.Add(test);
                        }
                        HasTests = true;
                        ShowNoTestsMessage = false;
                    }
                    else
                    {
                        Console.WriteLine("No tests found for company");
                        HasTests = false;
                        ShowNoTestsMessage = true;
                    }
                }
                else
                {
                    Console.WriteLine("Failed to retrieve tests from API");
                    HasTests = false;
                    ShowNoTestsMessage = true;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tests: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
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