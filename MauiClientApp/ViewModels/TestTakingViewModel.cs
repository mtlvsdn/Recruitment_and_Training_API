using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;
using System.Collections.ObjectModel;

namespace MauiClientApp.ViewModels
{
    public class TestTakingViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly SessionManager _sessionManager;
        private TestSession _testSession;
        private bool _isLoading;
        private string _statusMessage;
        private UserAnswer _currentUserAnswer;
        private string _timerDisplay;
        private bool _isTimerRunning;
        private IDispatcherTimer _timer;
        private DateTime _testEndTime;

        public TestSession TestSession 
        { 
            get => _testSession; 
            set
            {
                if (_testSession != value)
                {
                    _testSession = value;
                    OnPropertyChanged();
                }
            }
        }

        public UserAnswer CurrentUserAnswer
        {
            get => _currentUserAnswer;
            set
            {
                if (_currentUserAnswer != value)
                {
                    _currentUserAnswer = value;
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

        public string TimerDisplay
        {
            get => _timerDisplay;
            set
            {
                if (_timerDisplay != value)
                {
                    _timerDisplay = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsTimerRunning
        {
            get => _isTimerRunning;
            set
            {
                if (_isTimerRunning != value)
                {
                    _isTimerRunning = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand SelectAnswerCommand { get; }
        public ICommand NextQuestionCommand { get; }
        public ICommand PreviousQuestionCommand { get; }
        public ICommand SubmitTestCommand { get; }

        public TestTakingViewModel()
        {
            _apiService = new ApiService();
            _sessionManager = SessionManager.Instance;
            
            // Initialize TestSession with a new instance and ensure Test is also initialized
            _testSession = new TestSession();
            _testSession.Test = new Test();
            
            SelectAnswerCommand = new Command<string>(OnSelectAnswer);
            NextQuestionCommand = new Command(OnNextQuestion, CanGoToNextQuestion);
            PreviousQuestionCommand = new Command(OnPreviousQuestion, CanGoToPreviousQuestion);
            SubmitTestCommand = new Command(async () => await SubmitTestAsync());
            
            // Initialize timer display
            TimerDisplay = "00:00:00";
        }

        public async Task InitializeTestAsync(Test test)
        {
            if (test == null || test.test_id <= 0)
            {
                StatusMessage = "Invalid test data";
                Console.WriteLine("TestTakingViewModel: Cannot initialize test with null or invalid test data");
                await Application.Current.MainPage.DisplayAlert("Error", "Failed to start test: Invalid test data", "OK");
                return;
            }

            try
            {
                IsLoading = true;
                StatusMessage = "Loading test questions...";
                
                // Debug the test object
                Console.WriteLine($"TestTakingViewModel: Initializing test: {test.test_name} (ID: {test.test_id})");
                Console.WriteLine($"TestTakingViewModel: Number of questions: {test.no_of_questions}");
                Console.WriteLine($"TestTakingViewModel: Time limit: {test.time_limit} minutes");
                Console.WriteLine($"TestTakingViewModel: Company: {test.company_name}");
                
                // Make a copy of the test to ensure we don't modify the original
                var testCopy = new Test
                {
                    test_id = test.test_id,
                    test_name = !string.IsNullOrEmpty(test.test_name) ? test.test_name : $"Test {test.test_id}",
                    no_of_questions = test.no_of_questions > 0 ? test.no_of_questions : 1,
                    time_limit = test.time_limit > 0 ? test.time_limit : 10,
                    company_name = !string.IsNullOrEmpty(test.company_name) ? test.company_name : "Unknown Company"
                };
                
                // Initialize test session
                TestSession = new TestSession
                {
                    Test = testCopy,
                    StartTime = DateTime.Now,
                    UserAnswers = new ObservableCollection<UserAnswer>(),
                    CurrentQuestionIndex = 0,
                    IsCompleted = false,
                    CorrectAnswers = 0
                };
                
                _testEndTime = DateTime.Now.AddMinutes(testCopy.time_limit);
                
                // Fetch questions for this test
                Console.WriteLine($"TestTakingViewModel: Fetching questions for test {testCopy.test_id}");
                var questions = await _apiService.GetListAsync<Question>($"questions/test/{testCopy.test_id}");
                
                // Check if we got valid questions
                if (questions == null)
                {
                    StatusMessage = "Error: Failed to load test questions";
                    Console.WriteLine("TestTakingViewModel: API returned null instead of questions list");
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to load test questions", "OK");
                    return;
                }
                
                if (!questions.Any())
                {
                    StatusMessage = "This test has no questions";
                    Console.WriteLine("TestTakingViewModel: No questions found for this test");
                    await Application.Current.MainPage.DisplayAlert("Error", "This test has no questions", "OK");
                    return;
                }
                
                Console.WriteLine($"TestTakingViewModel: Loaded {questions.Count} questions for test {testCopy.test_id}");
                
                // Filter out any invalid questions first
                var validQuestions = questions
                    .Where(q => q != null && !string.IsNullOrEmpty(q.question_text) && !string.IsNullOrEmpty(q.correct_answer))
                    .ToList();
                    
                if (!validQuestions.Any())
                {
                    StatusMessage = "No valid questions found for this test";
                    Console.WriteLine("TestTakingViewModel: No valid questions found after filtering");
                    await Application.Current.MainPage.DisplayAlert("Error", "No valid questions found for this test", "OK");
                    return;
                }
                
                Console.WriteLine($"TestTakingViewModel: {validQuestions.Count} valid questions after filtering");
                
                // Sort questions and initialize user answers
                int questionCount = 0;
                foreach (var question in validQuestions.OrderBy(q => q.question_id))
                {
                    // Create a valid UserAnswer object with all properties set
                    var userAnswer = new UserAnswer
                    {
                        QuestionId = question.question_id,
                        Question = question,
                        IsAnswered = false,
                        SelectedAnswer = string.Empty,
                        IsCorrect = false
                    };
                    
                    TestSession.UserAnswers.Add(userAnswer);
                    questionCount++;
                    
                    // Only add up to the specified number of questions
                    if (questionCount >= testCopy.no_of_questions)
                        break;
                }
                
                // If we couldn't get enough questions, update the test properties
                if (TestSession.UserAnswers.Count < testCopy.no_of_questions)
                {
                    Console.WriteLine($"TestTakingViewModel: Not enough questions available. Expected {testCopy.no_of_questions}, got {TestSession.UserAnswers.Count}");
                    TestSession.Test.no_of_questions = TestSession.UserAnswers.Count;
                }
                
                // Check if we have any questions to display
                if (!TestSession.UserAnswers.Any())
                {
                    StatusMessage = "Failed to prepare test questions";
                    Console.WriteLine("TestTakingViewModel: No user answers created");
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to prepare test questions", "OK");
                    return;
                }
                
                // Set the first question as current
                try
                {
                    CurrentUserAnswer = TestSession.UserAnswers[0];
                    TestSession.CurrentQuestionIndex = 0;
                    Console.WriteLine($"TestTakingViewModel: Set first question: {CurrentUserAnswer.Question.question_text}");
                }
                catch (Exception ex)
                {
                    StatusMessage = "Error preparing first question";
                    Console.WriteLine($"TestTakingViewModel: Error setting first question: {ex.Message}");
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to start test: Error preparing questions", "OK");
                    return;
                }
                
                // Start the timer
                StartTimer();
                
                StatusMessage = "";
                Console.WriteLine("TestTakingViewModel: Test initialization completed successfully");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading test: {ex.Message}";
                Console.WriteLine($"TestTakingViewModel: Exception in InitializeTestAsync: {ex.Message}");
                Console.WriteLine($"TestTakingViewModel: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to start test: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private void StartTimer()
        {
            try
            {
                _timer = Application.Current.Dispatcher.CreateTimer();
                _timer.Interval = TimeSpan.FromSeconds(1);
                _timer.Tick += Timer_Tick;
                _timer.Start();
                IsTimerRunning = true;
                
                Console.WriteLine($"TestTakingViewModel: Timer started with {TestSession.Test.time_limit} minutes");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingViewModel: Error starting timer: {ex.Message}");
            }
        }

        private void Timer_Tick(object sender, EventArgs e)
        {
            try
            {
                var now = DateTime.Now;
                var timeRemaining = _testEndTime - now;
                
                if (timeRemaining.TotalSeconds <= 0)
                {
                    // Time's up!
                    StopTimer();
                    TimerDisplay = "00:00:00";
                    MainThread.BeginInvokeOnMainThread(async () => await HandleTimeUpAsync());
                    return;
                }
                
                // Update the timer display
                TimerDisplay = $"{timeRemaining.Hours:00}:{timeRemaining.Minutes:00}:{timeRemaining.Seconds:00}";
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingViewModel: Error in timer tick: {ex.Message}");
            }
        }

        private void StopTimer()
        {
            if (_timer != null)
            {
                _timer.Stop();
                _timer.Tick -= Timer_Tick;
                IsTimerRunning = false;
                Console.WriteLine("TestTakingViewModel: Timer stopped");
            }
        }

        private async Task HandleTimeUpAsync()
        {
            try
            {
                Console.WriteLine("TestTakingViewModel: Time's up!");
                
                // Mark any unanswered questions as incorrect
                foreach (var answer in TestSession.UserAnswers)
                {
                    if (!answer.IsAnswered)
                    {
                        answer.SelectedAnswer = "";
                        answer.IsCorrect = false;
                    }
                }
                
                // Complete the test and submit results
                TestSession.IsCompleted = true;
                TestSession.EndTime = DateTime.Now;
                
                await SubmitTestAsync();
                
                // Show a message and navigate to summary
                await Application.Current.MainPage.DisplayAlert(
                    "Time's Up!", 
                    "Your time has expired. The test will be submitted with your current answers.", 
                    "OK");
                
                await NavigateToUserDashboardAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingViewModel: Error handling time up: {ex.Message}");
            }
        }

        private async Task NavigateToUserDashboardAsync()
        {
            try
            {
                // Navigate back to the main dashboard
                await Shell.Current.GoToAsync("//dashboard");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"TestTakingViewModel: Error navigating to dashboard: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Error", 
                    "Failed to return to dashboard. Please restart the app.", 
                    "OK");
            }
        }

        private void OnSelectAnswer(string answer)
        {
            if (CurrentUserAnswer == null) return;
            
            Console.WriteLine($"TestTakingViewModel: Selected answer: {answer} for question {CurrentUserAnswer.QuestionId}");
            
            // Update the current answer
            CurrentUserAnswer.SelectedAnswer = answer;
            CurrentUserAnswer.IsAnswered = true;
            CurrentUserAnswer.IsCorrect = answer == CurrentUserAnswer.Question.correct_answer;
            
            // Update the command availability
            (NextQuestionCommand as Command)?.ChangeCanExecute();
            (PreviousQuestionCommand as Command)?.ChangeCanExecute();
        }

        private void OnNextQuestion()
        {
            if (!CanGoToNextQuestion()) return;
            
            TestSession.CurrentQuestionIndex++;
            if (TestSession.CurrentQuestionIndex < TestSession.UserAnswers.Count)
            {
                CurrentUserAnswer = TestSession.UserAnswers[TestSession.CurrentQuestionIndex];
                Console.WriteLine($"TestTakingViewModel: Moved to question {TestSession.CurrentQuestionIndex + 1} of {TestSession.UserAnswers.Count}");
            }
            
            // Update the command availability
            (NextQuestionCommand as Command)?.ChangeCanExecute();
            (PreviousQuestionCommand as Command)?.ChangeCanExecute();
        }

        private bool CanGoToNextQuestion()
        {
            return TestSession?.CurrentQuestionIndex < (TestSession?.UserAnswers?.Count - 1);
        }

        private void OnPreviousQuestion()
        {
            if (!CanGoToPreviousQuestion()) return;
            
            TestSession.CurrentQuestionIndex--;
            if (TestSession.CurrentQuestionIndex >= 0)
            {
                CurrentUserAnswer = TestSession.UserAnswers[TestSession.CurrentQuestionIndex];
                Console.WriteLine($"TestTakingViewModel: Moved to question {TestSession.CurrentQuestionIndex + 1} of {TestSession.UserAnswers.Count}");
            }
            
            // Update the command availability
            (NextQuestionCommand as Command)?.ChangeCanExecute();
            (PreviousQuestionCommand as Command)?.ChangeCanExecute();
        }

        private bool CanGoToPreviousQuestion()
        {
            return TestSession?.CurrentQuestionIndex > 0;
        }

        public async Task SubmitTestAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Submitting test results...";
                
                // Stop the timer
                StopTimer();
                
                // Complete the test session
                TestSession.IsCompleted = true;
                TestSession.EndTime = DateTime.Now;
                
                // Calculate the score
                TestSession.CorrectAnswers = TestSession.UserAnswers.Count(a => a.IsCorrect);
                
                // Log test completion details
                Console.WriteLine($"Test completed - User: {_sessionManager.UserId}, Test: {TestSession.Test.test_id}");
                Console.WriteLine($"Score: {TestSession.CorrectAnswers}/{TestSession.TotalQuestions}");
                Console.WriteLine($"Time taken: {TestSession.EndTime - TestSession.StartTime}");

                // Note: We'll let TestResultsViewModel handle saving the results
                // to prevent duplicate saves
                
                StatusMessage = "Test completed successfully!";
                Console.WriteLine("Test submission completed successfully");
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error submitting test: {ex.Message}";
                Console.WriteLine($"TestTakingViewModel: Error in SubmitTestAsync: {ex.Message}");
                Console.WriteLine($"TestTakingViewModel: Stack trace: {ex.StackTrace}");
                
                if (Application.Current?.MainPage != null)
                {
                    string errorDetails = $"Failed to submit test.\n\nError: {ex.Message}";
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

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 