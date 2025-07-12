using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;
using System.Collections.ObjectModel;

namespace MauiClientApp.ViewModels
{
    public partial class TestAnalyticsDetailViewModel : ObservableObject
    {
        private readonly ApiService _apiService;
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
        private ObservableCollection<TestResultDisplay> _testResults;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        [ObservableProperty]
        private bool _hasNoResults;

        public IRelayCommand LoadResultsCommand { get; }

        public TestAnalyticsDetailViewModel(int testId)
        {
            try
            {
                Console.WriteLine($"TestAnalyticsDetailViewModel: Constructor called with testId={testId}");
                
                _testId = testId;
                _apiService = new ApiService();
                TestResults = new ObservableCollection<TestResultDisplay>();
                
                LoadResultsCommand = new AsyncRelayCommand(LoadResults);
                
                Console.WriteLine("TestAnalyticsDetailViewModel: Initialization complete");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestAnalyticsDetailViewModel: Error in constructor: {ex.Message}");
                Console.WriteLine($"TestAnalyticsDetailViewModel: Stack trace: {ex.StackTrace}");
                throw;
            }
        }

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

                StatusMessage = string.Empty;
                HasNoResults = false;
                IsLoading = true;
                TestResults.Clear();

                // Get test details
                var test = await _apiService.GetAsync<Test>($"test/{_testId}");
                if (test == null)
                {
                    Console.WriteLine("TestAnalyticsDetailViewModel: Test details are null");
                    StatusMessage = "Test details could not be loaded.";
                    HasNoResults = true;
                    return;
                }

                TestName = test.test_name;
                TotalQuestions = test.no_of_questions;
                TimeLimit = test.time_limit;

                // Get test results
                var results = await _apiService.GetListAsync<Test_Results>($"test-results/by-test/{_testId}");
                if (results == null || !results.Any())
                {
                    Console.WriteLine("TestAnalyticsDetailViewModel: No results found");
                    HasNoResults = true;
                    StatusMessage = "No results found for this test.";
                    return;
                }

                // Get all users
                var users = await _apiService.GetListAsync<User>("user");
                var userDict = users?.ToDictionary(u => u.Id, u => u.Full_Name) ?? new Dictionary<int, string>();

                // Sort results by score (descending) and completion date
                var sortedResults = results
                    .OrderByDescending(r => r.score)
                    .ThenBy(r => r.completion_date);

                foreach (var result in sortedResults)
                {
                    var userName = userDict.TryGetValue(result.Userid, out var name) ? name : "Unknown User";
                    TestResults.Add(new TestResultDisplay
                    {
                        UserName = userName,
                        Score = result.score,
                        TotalQuestions = result.total_questions,
                        CompletionDate = result.completion_date
                    });
                }

                Console.WriteLine($"TestAnalyticsDetailViewModel: Loaded {TestResults.Count} results");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestAnalyticsDetailViewModel: Error in LoadResults: {ex.Message}");
                Console.WriteLine($"TestAnalyticsDetailViewModel: Stack trace: {ex.StackTrace}");
                StatusMessage = $"Failed to load results: {ex.Message}";
                HasNoResults = true;
            }
            finally
            {
                IsLoading = false;
            }
        }
    }
} 