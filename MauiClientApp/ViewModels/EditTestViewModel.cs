using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;
using System.Text;

namespace MauiClientApp.ViewModels
{
    public class EditTestViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private Test _test;
        private string _testTitle;
        private string _numberOfQuestions;
        private string _timeLimit;
        private bool _isLoading;

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

        public Test Test
        {
            get => _test;
            set
            {
                Console.WriteLine($"EditTestViewModel: Test property being set: {(value == null ? "null" : $"ID: {value.test_id}")}");
                if (_test != value)
                {
                    _test = value;
                    OnPropertyChanged();
                    
                    // Always load test data if we have a valid ID, regardless of other properties
                    if (_test != null && _test.test_id > 0)
                    {
                        Console.WriteLine($"EditTestViewModel: Starting LoadTestDataAsync for test ID: {_test.test_id}");
                        LoadTestDataAsync().ConfigureAwait(false);
                    }
                    else
                    {
                        Console.WriteLine("EditTestViewModel: Test has invalid ID, not loading data");
                    }
                }
                else
                {
                    Console.WriteLine("EditTestViewModel: Test property value unchanged, not triggering reload");
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

        public string NumberOfQuestions
        {
            get => _numberOfQuestions;
            set
            {
                if (_numberOfQuestions != value)
                {
                    _numberOfQuestions = value;
                    OnPropertyChanged();
                }
            }
        }

        public string TimeLimit
        {
            get => _timeLimit;
            set
            {
                if (_timeLimit != value)
                {
                    _timeLimit = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }
        public ICommand ViewQuestionsCommand { get; }

        public EditTestViewModel()
        {
            Console.WriteLine("EditTestViewModel: Constructor called");
            _apiService = new ApiService();
            // Initialize Test with a default instance to avoid null reference exceptions
            _test = new Test();
            SaveCommand = new Command(async () => await SaveTestAsync());
            CancelCommand = new Command(async () => await CancelAsync());
            ViewQuestionsCommand = new Command(async () => await ViewQuestionsAsync());
            Console.WriteLine("EditTestViewModel: Initialized with API service and commands");
        }

        private async Task LoadTestDataAsync()
        {
            Console.WriteLine($"EditTestViewModel: LoadTestDataAsync started for test ID: {Test?.test_id}");
            if (Test == null)
            {
                Console.WriteLine("EditTestViewModel: Test is null, cannot load data");
                return;
            }
            
            if (Test.test_id <= 0)
            {
                Console.WriteLine("EditTestViewModel: Test ID is <= 0, cannot load data");
                return;
            }

            try
            {
                Console.WriteLine($"EditTestViewModel: Setting IsLoading to true");
                IsLoading = true;
                
                Console.WriteLine($"EditTestViewModel: Calling API to get test data for ID: {Test.test_id}");
                var testData = await _apiService.GetAsync<Test>($"test/{Test.test_id}");
                
                if (testData != null)
                {
                    Console.WriteLine($"EditTestViewModel: API returned test data: {testData.test_name}");
                    // Update on main thread
                    await MainThread.InvokeOnMainThreadAsync(() =>
                    {
                        Console.WriteLine("EditTestViewModel: Updating UI on main thread");
                        // First update the properties that are bound to the UI
                        TestTitle = testData.test_name;
                        NumberOfQuestions = testData.no_of_questions.ToString();
                        TimeLimit = testData.time_limit.ToString();
                        
                        // Then update the Test object itself
                        _test.test_name = testData.test_name;
                        _test.no_of_questions = testData.no_of_questions;
                        _test.time_limit = testData.time_limit;
                        _test.company_name = testData.company_name;
                        
                        Console.WriteLine("EditTestViewModel: UI update completed");
                    });
                }
                else
                {
                    Console.WriteLine("EditTestViewModel: API returned null test data");
                    await MainThread.InvokeOnMainThreadAsync(async () =>
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Failed to load test data", "OK");
                        await SafeNavigateBack();
                    });
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditTestViewModel: Exception during data load: {ex.Message}");
                Console.WriteLine($"EditTestViewModel: Stack trace: {ex.StackTrace}");
                await MainThread.InvokeOnMainThreadAsync(async () =>
                {
                    await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load test data: {ex.Message}", "OK");
                    await SafeNavigateBack();
                });
            }
            finally
            {
                IsLoading = false;
                Console.WriteLine("EditTestViewModel: LoadTestDataAsync completed, IsLoading set to false");
            }
        }

        private async Task SaveTestAsync()
        {
            if (string.IsNullOrWhiteSpace(TestTitle) ||
                string.IsNullOrWhiteSpace(NumberOfQuestions) ||
                string.IsNullOrWhiteSpace(TimeLimit))
            {
                await Application.Current.MainPage.DisplayAlert("Error", "All fields are required", "OK");
                return;
            }

            if (!int.TryParse(NumberOfQuestions, out int questions) || questions <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Number of questions must be a positive integer", "OK");
                return;
            }

            if (!int.TryParse(TimeLimit, out int timeLimit) || timeLimit <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Time limit must be a positive integer", "OK");
                return;
            }

            if (Test == null || Test.test_id <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Invalid test data. Please try again.", "OK");
                await SafeNavigateBack();
                return;
            }

            try
            {
                Console.WriteLine($"EditTestViewModel: Saving test with ID: {Test.test_id}");
                IsLoading = true;
                var updatedTest = new Test
                {
                    test_id = Test.test_id,
                    test_name = TestTitle,
                    no_of_questions = questions,
                    time_limit = timeLimit,
                    // Preserve company_name if it exists
                    company_name = Test.company_name ?? SessionManager.Instance?.CompanyName ?? string.Empty
                };

                var result = await _apiService.PutAsync<Test>($"test/{Test.test_id}", updatedTest);
                if (result != null)
                {
                    await Application.Current.MainPage.DisplayAlert("Success", "Test updated successfully", "OK");
                    
                    // Navigate to question editing or go back
                    if (await Application.Current.MainPage.DisplayAlert(
                        "Edit Questions", 
                        "Would you like to edit the questions for this test?", 
                        "Yes", "No"))
                    {
                        // Navigate to first question
                        await NavigateToEditQuestions();
                    }
                    else
                    {
                        // Go back using safe navigation
                        await SafeNavigateBack();
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to update test", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditTestViewModel: Error saving test: {ex.Message}");
                Console.WriteLine($"EditTestViewModel: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task NavigateToEditQuestions()
        {
            try
            {
                // Make sure we have all the required parameters
                if (Test == null || Test.test_id <= 0 || Test.no_of_questions <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Invalid test data for question editing", "OK");
                    await SafeNavigateBack();
                    return;
                }

                Console.WriteLine($"EditTestViewModel: Navigating to edit questions for test: {Test.test_id}");
                
                // First navigate back from the edit test page
                await SafeNavigateBack();
                
                // Now try to create and navigate to the first question page
                try
                {
                    // Create edit question page for the first question
                    var page = new MauiClientApp.Views.Tests.CreateQuestionPage();
                    var parameters = new Dictionary<string, object>
                    {
                        { "Test", Test },
                        { "questionNumber", 1 },
                        { "totalQuestions", Test.no_of_questions },
                        { "IsEditMode", true }
                    };
                    
                    // Apply parameters using IQueryAttributable
                    ((IQueryAttributable)page).ApplyQueryAttributes(parameters);
                    
                    // Navigate to the first question
                    await Application.Current.MainPage.Navigation.PushAsync(page);
                    
                    Console.WriteLine("EditTestViewModel: Navigation to edit questions completed");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"EditTestViewModel: Error during question page creation/navigation: {ex.Message}");
                    Console.WriteLine($"EditTestViewModel: Stack trace: {ex.StackTrace}");
                    await Application.Current.MainPage.DisplayAlert("Error", 
                        "Failed to navigate to question editor. You'll be returned to the main page.", "OK");
                    Application.Current.MainPage = new AppShell();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditTestViewModel: Error navigating to edit questions: {ex.Message}");
                Console.WriteLine($"EditTestViewModel: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to navigate to question editor", "OK");
                await SafeNavigateBack();
            }
        }

        private async Task SafeNavigateBack()
        {
            try
            {
                if (Application.Current.MainPage.Navigation.NavigationStack.Count > 1)
                {
                    await Application.Current.MainPage.Navigation.PopAsync();
                }
                else
                {
                    // If we're somehow at the root of the navigation stack, just reset to app shell
                    Application.Current.MainPage = new AppShell();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditTestViewModel: Error in SafeNavigateBack: {ex.Message}");
                Console.WriteLine($"EditTestViewModel: Stack trace: {ex.StackTrace}");
                
                // Last resort - reset to app shell
                Application.Current.MainPage = new AppShell();
            }
        }

        private async Task CancelAsync()
        {
            // Go back using safe navigation
            await SafeNavigateBack();
        }

        private async Task ViewQuestionsAsync()
        {
            if (Test == null || Test.test_id <= 0)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "Please save the test first before viewing questions.", "OK");
                return;
            }

            try
            {
                Console.WriteLine($"EditTestViewModel: Loading questions for test ID: {Test.test_id}");
                IsLoading = true;

                // Load questions for this test
                var questions = await _apiService.GetListAsync<Question>($"questions/test/{Test.test_id}");
                
                if (questions == null || !questions.Any())
                {
                    await Application.Current.MainPage.DisplayAlert("Information", "No questions found for this test.", "OK");
                    return;
                }

                // Order questions by ID to see the most recent versions first
                var orderedQuestions = questions.OrderByDescending(q => q.question_id).ToList();
                
                // Create a simple display of questions
                var questionsText = new StringBuilder();
                questionsText.AppendLine($"Test: {Test.test_name}");
                questionsText.AppendLine($"Total Questions: {orderedQuestions.Count}");
                questionsText.AppendLine();
                
                for (int i = 0; i < orderedQuestions.Count; i++)
                {
                    var q = orderedQuestions[i];
                    questionsText.AppendLine($"Question {i+1} (ID: {q.question_id}):");
                    questionsText.AppendLine($"• {q.question_text}");
                    questionsText.AppendLine($"  A: {q.possible_answer_1}");
                    questionsText.AppendLine($"  B: {q.possible_answer_2}");
                    questionsText.AppendLine($"  C: {q.possible_answer_3}");
                    questionsText.AppendLine($"  D: {q.possible_answer_4}");
                    questionsText.AppendLine($"  Correct: {q.correct_answer}");
                    questionsText.AppendLine();
                }
                
                // Show the questions
                await Application.Current.MainPage.DisplayAlert(
                    $"Questions for {Test.test_name}", 
                    questionsText.ToString(),
                    "OK");
                
                // Inform user about duplicate questions issue
                if (orderedQuestions.Count > Test.no_of_questions)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "Note",
                        "There are more questions than expected. This may be due to duplicate questions created during updates. " +
                        "Currently the API doesn't support deleting questions. The most recent versions will be used when taking the test.",
                        "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"EditTestViewModel: Error viewing questions: {ex.Message}");
                Console.WriteLine($"EditTestViewModel: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load questions: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 