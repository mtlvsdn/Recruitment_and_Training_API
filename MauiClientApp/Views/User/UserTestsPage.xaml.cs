using MauiClientApp.ViewModels;
using MauiClientApp.Services;
using MauiClientApp.Models;
using System.Collections.Generic;
using System.Collections.ObjectModel;

namespace MauiClientApp.Views.User
{
    public partial class UserTestsPage : ContentPage
    {
        private UserTestsViewModel _viewModel;
        
        // Use a static flag to track initialization across page instances
        private static bool _pageInitialized = false;

        public UserTestsPage()
        {
            Console.WriteLine("UserTestsPage: Constructor called");
            
            try
            {
                InitializeComponent();
                
                // Always create a new ViewModel instance to guarantee a clean state
                _viewModel = new UserTestsViewModel();
                BindingContext = _viewModel;
                
                Console.WriteLine("UserTestsPage: Page constructed with new ViewModel");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserTestsPage: Exception in constructor: {ex.Message}");
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                Console.WriteLine("UserTestsPage: OnAppearing called");
                
                // Only load tests once per app session
                if (!_pageInitialized)
                {
                    Console.WriteLine("UserTestsPage: First appearance ever, loading tests");
                    
                    // Use Device.BeginInvokeOnMainThread to ensure UI updates happen on the main thread
                    MainThread.BeginInvokeOnMainThread(() =>
                    {
                        LoadTestsOnce();
                        _pageInitialized = true;
                        
                        // Attach event handlers to Start Test buttons after tests are loaded
                        AttachStartTestButtonHandlers();
                    });
                }
                else
                {
                    Console.WriteLine("UserTestsPage: Page already initialized in this session, skipping automatic load");
                    // Still attach event handlers in case they need to be refreshed
                    MainThread.BeginInvokeOnMainThread(() => AttachStartTestButtonHandlers());
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserTestsPage: Exception in OnAppearing: {ex.Message}");
            }
        }
        
        private void LoadTestsOnce()
        {
            try
            {
                Console.WriteLine("UserTestsPage: LoadTestsOnce called");
                
                if (_viewModel == null)
                {
                    Console.WriteLine("UserTestsPage: ERROR - ViewModel is null, creating new instance");
                    _viewModel = new UserTestsViewModel();
                    BindingContext = _viewModel;
                }
                
                // Explicitly clear the Tests collection first
                _viewModel.Tests.Clear();
                
                // Execute the refresh command to load tests
                _viewModel.RefreshCommand.Execute(null);
                
                Console.WriteLine("UserTestsPage: Tests load initiated");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserTestsPage: Exception in LoadTestsOnce: {ex.Message}");
            }
        }

        // Handle the refresh button click
        private void RefreshButton_Clicked(object sender, EventArgs e)
        {
            try
            {
                Console.WriteLine("UserTestsPage: Manual refresh requested");
                
                // For manual refreshes, we want to force a reload regardless of the initialization state
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    if (_viewModel != null)
                    {
                        _viewModel.Tests.Clear();
                        _viewModel.RefreshCommand.Execute(null);
                        Console.WriteLine("UserTestsPage: Manual refresh executed");
                    }
                    else
                    {
                        Console.WriteLine("UserTestsPage: ERROR - Cannot refresh, ViewModel is null");
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserTestsPage: Exception in RefreshButton_Clicked: {ex.Message}");
            }
        }

        // Helper method to attach handlers to Start Test buttons
        private void AttachStartTestButtonHandlers()
        {
            try
            {
                Console.WriteLine("UserTestsPage: No longer needed to attach event handlers as we're using direct Click events");
                // We're now using direct Click events defined in XAML
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserTestsPage: Error in AttachStartTestButtonHandlers: {ex.Message}");
            }
        }

        // Direct click handler for Start Test buttons - new direct approach
        private async void StartTest_Clicked(object sender, EventArgs e)
        {
            try
            {
                if (sender is Button button && button.BindingContext is Models.Test test)
                {
                    Console.WriteLine($"UserTestsPage: Start Test clicked for test: {test.test_name} (ID: {test.test_id})");
                    
                    // Instead of navigating to TestTakingPage, we directly load the first question
                    var apiService = new ApiService();
                    
                    // Step 1: Get the test details to confirm it exists
                    Console.WriteLine($"UserTestsPage: Fetching test data for ID: {test.test_id}");
                    var testData = await apiService.GetAsync<Test>($"test/{test.test_id}");
                    
                    if (testData == null)
                    {
                        Console.WriteLine("UserTestsPage: Failed to fetch test data");
                        await DisplayAlert("Error", "Could not load test data.", "OK");
                        return;
                    }
                    
                    // Step 2: Get the questions for this test
                    Console.WriteLine($"UserTestsPage: Fetching questions for test ID: {test.test_id}");
                    var questions = await apiService.GetListAsync<Question>($"questions/test/{test.test_id}");
                    
                    if (questions == null || !questions.Any())
                    {
                        Console.WriteLine("UserTestsPage: No questions found for this test");
                        await DisplayAlert("Error", "This test does not have any questions.", "OK");
                        return;
                    }
                    
                    Console.WriteLine($"UserTestsPage: Found {questions.Count} questions for the test");
                    
                    // Step 3: Create a TestSession to track progress
                    var testSession = new TestSession
                    {
                        Test = testData,
                        Questions = questions.ToList(),
                        CurrentQuestionIndex = 0,
                        UserAnswers = new ObservableCollection<UserAnswer>(),
                        StartTime = DateTime.Now,
                        IsCompleted = false
                    };
                    
                    // Step 4: Navigate directly to the question page with all required data
                    var questionPage = new Views.Tests.QuestionPage(testSession);
                    await Navigation.PushAsync(questionPage);
                    
                    Console.WriteLine("UserTestsPage: Navigated directly to the question page");
                }
                else
                {
                    Console.WriteLine("UserTestsPage: Start Test clicked but couldn't get test data");
                    if (sender is Button btn)
                    {
                        Console.WriteLine($"UserTestsPage: Button BindingContext type: {btn.BindingContext?.GetType().Name ?? "null"}");
                    }
                    await DisplayAlert("Error", "Cannot determine which test to start", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"UserTestsPage: Error in StartTest_Clicked: {ex.Message}");
                Console.WriteLine($"UserTestsPage: Stack trace: {ex.StackTrace}");
                await DisplayAlert("Error", $"Failed to start test: {ex.Message}", "OK");
            }
        }
    }
} 