using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;
using System.Collections.ObjectModel;

namespace MauiClientApp.ViewModels
{
    public partial class TestAnalyticsDetailViewModel : ObservableObject
    {
        private readonly ITestService _testService;
        private readonly int _testId;

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private string _testName = string.Empty;

        [ObservableProperty]
        private int _totalQuestions;

        [ObservableProperty]
        private int _timeLimit;

        [ObservableProperty]
        private ObservableCollection<TestUserResult> _userResults;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _hasNoUsers;

        // Explicitly declare the command to ensure it's properly initialized
        public IRelayCommand LoadResultsCommand { get; }

        public TestAnalyticsDetailViewModel(int testId)
        {
            try
            {
                Console.WriteLine($"TestAnalyticsDetailViewModel: Constructor called with testId={testId}");
                
                // Initialize properties
                _testId = testId;
                UserResults = new ObservableCollection<TestUserResult>();
                
                // Get the test service
                Console.WriteLine("TestAnalyticsDetailViewModel: Getting ITestService");
                _testService = ServiceHelper.GetService<ITestService>();
                
                if (_testService == null)
                {
                    Console.WriteLine("TestAnalyticsDetailViewModel: ERROR - _testService is null");
                    throw new InvalidOperationException("ITestService could not be resolved");
                }
                
                Console.WriteLine("TestAnalyticsDetailViewModel: ITestService successfully retrieved");
                
                // Initialize the command
                Console.WriteLine("TestAnalyticsDetailViewModel: Initializing LoadResultsCommand");
                LoadResultsCommand = new AsyncRelayCommand(LoadResults);
                Console.WriteLine("TestAnalyticsDetailViewModel: LoadResultsCommand initialized");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestAnalyticsDetailViewModel: Error in constructor: {ex.Message}");
                Console.WriteLine($"TestAnalyticsDetailViewModel: Stack trace: {ex.StackTrace}");
                throw; // Re-throw to make the error visible to the caller
            }
        }

        // Removed [RelayCommand] attribute since we're manually creating the command
        private async Task LoadResults()
        {
            try
            {
                Console.WriteLine("TestAnalyticsDetailViewModel: LoadResults called");
                
                if (IsLoading)
                {
                    Console.WriteLine("TestAnalyticsDetailViewModel: Already loading, returning");
                    return;
                }

                // Reset state
                StatusMessage = string.Empty;
                HasNoUsers = false;

                Console.WriteLine("TestAnalyticsDetailViewModel: Setting IsLoading=true");
                IsLoading = true;
                
                Console.WriteLine("TestAnalyticsDetailViewModel: Clearing results");
                UserResults.Clear();

                Console.WriteLine($"TestAnalyticsDetailViewModel: Getting test details for testId={_testId}");
                var testDetails = await _testService.GetTestDetailsAsync(_testId);
                
                if (testDetails == null)
                {
                    Console.WriteLine("TestAnalyticsDetailViewModel: Test details are null");
                    StatusMessage = "Test details could not be loaded.";
                    return;
                }
                
                Console.WriteLine($"TestAnalyticsDetailViewModel: Got test details - name={testDetails.test_name}");
                TestName = testDetails.test_name ?? "Unknown Test";
                TotalQuestions = testDetails.no_of_questions;
                TimeLimit = testDetails.time_limit;

                try
                {
                    // Get all users assigned to this test
                    var users = await _testService.GetTestUsersAsync(_testId);
                    
                    if (users == null || !users.Any())
                    {
                        Console.WriteLine("TestAnalyticsDetailViewModel: No users found for this test");
                        HasNoUsers = true;
                        StatusMessage = "No users are assigned to this test yet. Assigned users would appear here.";
                        return;
                    }
                    
                    // Get test results (completed tests)
                    var results = await _testService.GetTestResultsAsync(_testId);
                    
                    // Process and rank results if they exist
                    var rankedResults = new List<TestResult>();
                    if (results != null && results.Any())
                    {
                        // Sort by score and time
                        rankedResults = results
                            .OrderByDescending(r => r.Score)
                            .ThenBy(r => r.TimeSpent)
                            .ToList();
                            
                        // Assign ranks
                        for (int i = 0; i < rankedResults.Count; i++)
                        {
                            rankedResults[i].Rank = i + 1;
                        }
                    }
                    
                    // Create combined user-result models
                    foreach (var user in users)
                    {
                        // Try to find a result for this user
                        var userResult = rankedResults.FirstOrDefault(r => 
                            r.CandidateEmail.Equals(user.Email, StringComparison.OrdinalIgnoreCase));
                            
                        var testUserResult = new TestUserResult
                        {
                            UserId = user.Id,
                            FullName = user.Full_Name,
                            Email = user.Email,
                            HasTakenTest = userResult != null
                        };
                        
                        // If the user has taken the test, add the details
                        if (userResult != null)
                        {
                            testUserResult.Rank = userResult.Rank;
                            testUserResult.Score = userResult.Score;
                            testUserResult.TimeSpent = userResult.TimeSpent;
                            testUserResult.CompletedOn = userResult.CompletedOn;
                        }
                        
                        UserResults.Add(testUserResult);
                    }
                    
                    // Sort the collection - completed tests first, then by rank
                    var sortedResults = UserResults.OrderByDescending(r => r.HasTakenTest)
                        .ThenBy(r => r.Rank ?? int.MaxValue)
                        .ToList();
                        
                    UserResults.Clear();
                    foreach (var result in sortedResults)
                    {
                        UserResults.Add(result);
                    }
                    
                    Console.WriteLine($"TestAnalyticsDetailViewModel: Displayed {UserResults.Count} user results");
                }
                catch (Exception ex)
                {
                    // Handle errors specifically for test results without failing the whole view
                    Console.WriteLine($"TestAnalyticsDetailViewModel: Error loading results: {ex.Message}");
                    HasNoUsers = true;
                    StatusMessage = $"Could not retrieve users for this test. Error: {ex.Message}";
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestAnalyticsDetailViewModel: Error in LoadResults: {ex.Message}");
                Console.WriteLine($"TestAnalyticsDetailViewModel: Stack trace: {ex.StackTrace}");
                StatusMessage = $"Failed to load data: {ex.Message}";
            }
            finally
            {
                Console.WriteLine("TestAnalyticsDetailViewModel: Setting IsLoading=false");
                IsLoading = false;
            }
        }
    }
} 