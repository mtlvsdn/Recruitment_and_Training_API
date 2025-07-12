using MauiClientApp.Models;
using MauiClientApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class TestResultsViewModel : INotifyPropertyChanged
    {
        private readonly ITestService _testService;
        private readonly ApiService _apiService;
        private TestSession _testSession;
        private string _testTitle;
        private string _scoreDisplay;
        private string _scorePercentage;
        private string _timeTaken;
        private bool _isLoading;

        public TestSession TestSession
        {
            get => _testSession;
            set
            {
                if (_testSession != value)
                {
                    _testSession = value;
                    OnPropertyChanged();
                    UpdateDisplayProperties();
                }
            }
        }

        public string TestTitle
        {
            get => _testTitle;
            set
            {
                if (_testTitle != value)
                {
                    _testTitle = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ScoreDisplay
        {
            get => _scoreDisplay;
            set
            {
                if (_scoreDisplay != value)
                {
                    _scoreDisplay = value;
                    OnPropertyChanged();
                }
            }
        }

        public string ScorePercentage
        {
            get => _scorePercentage;
            set
            {
                if (_scorePercentage != value)
                {
                    _scorePercentage = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TimeTaken
        {
            get => _timeTaken;
            set
            {
                if (_timeTaken != value)
                {
                    _timeTaken = value;
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

        public ICommand ReturnToDashboardCommand { get; }

        public TestResultsViewModel(TestSession testSession)
        {
            _testService = ServiceHelper.GetService<ITestService>();
            _apiService = ServiceHelper.GetService<ApiService>();
            TestSession = testSession;
            ReturnToDashboardCommand = new Command(async () => await ReturnToDashboard());
            UpdateDisplayProperties();
            SaveTestResults();
        }

        private void UpdateDisplayProperties()
        {
            if (TestSession == null) return;

            TestTitle = TestSession.Test?.test_name ?? "Test Results";
            
            // Calculate score percentage
            double percentage = (double)TestSession.CorrectAnswers / TestSession.TotalQuestions * 100;
            ScorePercentage = $"{percentage:0.#}%";
            ScoreDisplay = $"{TestSession.CorrectAnswers} out of {TestSession.TotalQuestions} correct";

            // Calculate time taken
            var timeSpent = TestSession.EndTime - TestSession.StartTime;
            TimeTaken = $"{(int)timeSpent.TotalMinutes:00}:{timeSpent.Seconds:00}";
        }

        private async void SaveTestResults()
        {
            try
            {
                IsLoading = true;

                // Check if test session is valid
                if (TestSession == null || TestSession.Test == null)
                {
                    throw new InvalidOperationException("Test session or test data is missing");
                }

                // Create test result object that exactly matches the database schema
                var testResult = new Test_Results
                {
                    Userid = SessionManager.Instance.UserId,
                    Testtest_id = TestSession.Test.test_id,
                    completion_date = TestSession.EndTime,
                    score = TestSession.CorrectAnswers,
                    total_questions = TestSession.TotalQuestions
                };

                Console.WriteLine($"Attempting to save test results - User: {testResult.Userid}, Test: {testResult.Testtest_id}, Score: {testResult.score}/{testResult.total_questions}");

                // Save the test result to the database
                var response = await _apiService.PostAsync<Test_Results>("test-results", testResult);
                
                if (response == null)
                {
                    throw new Exception("No response received from the API");
                }

                Console.WriteLine($"Test results saved successfully. Result ID: {response.result_id}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error saving test results: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                
                // Show a more detailed error message to help with troubleshooting
                if (Application.Current?.MainPage != null)
                {
                    string errorDetails = $"Failed to save test results.\n\nError: {ex.Message}";
                    if (ex.InnerException != null)
                    {
                        errorDetails += $"\n\nAdditional details: {ex.InnerException.Message}";
                    }
                    
                    await Application.Current.MainPage.DisplayAlert(
                        "Error", 
                        errorDetails,
                        "OK");
                }
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ReturnToDashboard()
        {
            try
            {
                // Navigate to the user dashboard using the navigation stack
                await Application.Current.MainPage.Navigation.PushAsync(new Views.UserDashboardPage());
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to dashboard: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to navigate to dashboard", "OK");
                }
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 