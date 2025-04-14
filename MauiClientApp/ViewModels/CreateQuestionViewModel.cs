using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;
using MauiClientApp.Views.Tests;
using MauiClientApp.Services;

namespace MauiClientApp.ViewModels
{
    [QueryProperty(nameof(QuestionNumber), "questionNumber")]
    [QueryProperty(nameof(TotalQuestions), "totalQuestions")]
    [QueryProperty(nameof(Test), "Test")]
    [QueryProperty(nameof(IsEditMode), "IsEditMode")]
    public class CreateQuestionViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private readonly ApiService _apiService;
        private int _questionNumber;
        private int _totalQuestions;
        private Test _test;
        private string _questionText;
        private string _answerA;
        private string _answerB;
        private string _answerC;
        private string _answerD;
        private bool _isNextEnabled;
        private string _correctAnswer;
        private bool _isEditMode;
        private int _questionId;
        private bool _isQuestionLoaded;

        public bool IsQuestionLoaded
        {
            get => _isQuestionLoaded;
            set
            {
                if (_isQuestionLoaded != value)
                {
                    _isQuestionLoaded = value;
                    OnPropertyChanged();
                }
            }
        }

        public int QuestionId
        {
            get => _questionId;
            set
            {
                if (_questionId != value)
                {
                    _questionId = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsEditMode
        {
            get => _isEditMode;
            set
            {
                if (_isEditMode != value)
                {
                    _isEditMode = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(ActionButtonText));
                }
            }
        }

        public string ActionButtonText => IsEditMode ? "Update" : "Next";

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Console.WriteLine($"CreateQuestionViewModel: ApplyQueryAttributes called with {query.Count} parameters");
            
            if (query.ContainsKey("questionNumber") && int.TryParse(query["questionNumber"].ToString(), out int questionNumber))
            {
                QuestionNumber = questionNumber;
                Console.WriteLine($"CreateQuestionViewModel: QuestionNumber set to {QuestionNumber}");
            }
            
            if (query.ContainsKey("totalQuestions") && int.TryParse(query["totalQuestions"].ToString(), out int totalQuestions))
            {
                TotalQuestions = totalQuestions;
                Console.WriteLine($"CreateQuestionViewModel: TotalQuestions set to {TotalQuestions}");
            }
            
            if (query.ContainsKey("Test"))
            {
                Test = query["Test"] as Test;
                Console.WriteLine($"CreateQuestionViewModel: Test set to {Test?.test_name ?? "null"}");
            }

            if (query.ContainsKey("IsEditMode") && query["IsEditMode"] is bool editMode)
            {
                IsEditMode = editMode;
                Console.WriteLine($"CreateQuestionViewModel: IsEditMode set to {IsEditMode}");
            }
        }

        public int QuestionNumber
        {
            get => _questionNumber;
            set
            {
                if (_questionNumber != value)
                {
                    _questionNumber = value;
                    OnPropertyChanged();
                    OnPropertyChanged(nameof(PageTitle));
                }
            }
        }

        public int TotalQuestions
        {
            get => _totalQuestions;
            set
            {
                if (_totalQuestions != value)
                {
                    _totalQuestions = value;
                    OnPropertyChanged();
                }
            }
        }

        public Test Test
        {
            get => _test;
            set
            {
                if (_test != value)
                {
                    _test = value;
                    OnPropertyChanged();
                }
            }
        }

        public string PageTitle => $"Question {QuestionNumber} of {TotalQuestions}";

        public string QuestionText
        {
            get => _questionText;
            set
            {
                if (_questionText != value)
                {
                    _questionText = value;
                    OnPropertyChanged();
                    ValidateInputs();
                }
            }
        }

        public string AnswerA
        {
            get => _answerA;
            set
            {
                if (_answerA != value)
                {
                    _answerA = value;
                    OnPropertyChanged();
                    ValidateInputs();
                }
            }
        }

        public string AnswerB
        {
            get => _answerB;
            set
            {
                if (_answerB != value)
                {
                    _answerB = value;
                    OnPropertyChanged();
                    ValidateInputs();
                }
            }
        }

        public string AnswerC
        {
            get => _answerC;
            set
            {
                if (_answerC != value)
                {
                    _answerC = value;
                    OnPropertyChanged();
                    ValidateInputs();
                }
            }
        }

        public string AnswerD
        {
            get => _answerD;
            set
            {
                if (_answerD != value)
                {
                    _answerD = value;
                    OnPropertyChanged();
                    ValidateInputs();
                }
            }
        }

        public string CorrectAnswer
        {
            get => _correctAnswer;
            set
            {
                if (_correctAnswer != value)
                {
                    _correctAnswer = value;
                    OnPropertyChanged();
                    ValidateInputs();
                }
            }
        }

        public bool IsNextEnabled
        {
            get => _isNextEnabled;
            set
            {
                if (_isNextEnabled != value)
                {
                    _isNextEnabled = value;
                    OnPropertyChanged();
                    (NextCommand as Command)?.ChangeCanExecute();
                }
            }
        }

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }

        public CreateQuestionViewModel()
        {
            _apiService = new ApiService();
            NextCommand = new Command(async () => await OnNext(), () => IsNextEnabled);
            BackCommand = new Command(OnBack);
            
            // Initialize with empty values
            QuestionText = string.Empty;
            AnswerA = string.Empty;
            AnswerB = string.Empty;
            AnswerC = string.Empty;
            AnswerD = string.Empty;
            CorrectAnswer = string.Empty;
            IsNextEnabled = false;
            IsQuestionLoaded = false;
        }

        private void ValidateInputs()
        {
            // Validate all inputs are provided
            IsNextEnabled = !string.IsNullOrWhiteSpace(QuestionText) &&
                            !string.IsNullOrWhiteSpace(AnswerA) &&
                            !string.IsNullOrWhiteSpace(AnswerB) &&
                            !string.IsNullOrWhiteSpace(AnswerC) &&
                            !string.IsNullOrWhiteSpace(AnswerD) &&
                            !string.IsNullOrWhiteSpace(CorrectAnswer);
        }

        private async Task OnNext()
        {
            try
            {
                if (IsEditMode)
                {
                    await UpdateQuestion();
                }
                else
                {
                    await SaveNewQuestion();
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateQuestionViewModel: Error in OnNext: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async Task SaveNewQuestion()
        {
            try
            {
                // Create question object
                var question = new Question
                {
                    test_id = Test.test_id,
                    question_text = QuestionText,
                    possible_answer_1 = AnswerA,
                    possible_answer_2 = AnswerB,
                    possible_answer_3 = AnswerC,
                    possible_answer_4 = AnswerD,
                    correct_answer = CorrectAnswer
                };

                // Save to API
                var result = await _apiService.PostAsync<Question>("questions", question);

                if (result != null)
                {
                    // Clear inputs for next question
                    QuestionText = string.Empty;
                    AnswerA = string.Empty;
                    AnswerB = string.Empty;
                    AnswerC = string.Empty;
                    AnswerD = string.Empty;
                    CorrectAnswer = string.Empty;

                    // Check if we've reached the last question
                    if (QuestionNumber < TotalQuestions)
                    {
                        // Navigate to next question by simply incrementing the counter
                        // since we're staying on the same page
                        QuestionNumber++;
                    }
                    else
                    {
                        // All questions completed
                        await Application.Current.MainPage.DisplayAlert("Success", "All questions created successfully!", "OK");
                        await GoBackToTestsList();
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to save question", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateQuestionViewModel: Error saving question: {ex.Message}");
                Console.WriteLine($"CreateQuestionViewModel: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save question: {ex.Message}", "OK");
            }
        }

        private async Task UpdateQuestion()
        {
            try
            {
                Console.WriteLine($"CreateQuestionViewModel: Updating question ID: {QuestionId}");
                
                // Create question object with the ID of the existing question
                var updatedQuestion = new Question
                {
                    question_id = QuestionId, // Include the existing ID for update
                    test_id = Test.test_id,
                    question_text = QuestionText,
                    possible_answer_1 = AnswerA,
                    possible_answer_2 = AnswerB,
                    possible_answer_3 = AnswerC,
                    possible_answer_4 = AnswerD,
                    correct_answer = CorrectAnswer
                };

                Console.WriteLine($"API Request - PUT to questions/{QuestionId}");
                var result = await _apiService.PutAsync<Question>($"questions/{QuestionId}", updatedQuestion);
                
                if (result != null)
                {
                    Console.WriteLine($"CreateQuestionViewModel: Question updated successfully with ID: {result.question_id}");
                    await Application.Current.MainPage.DisplayAlert("Success", "Question updated successfully!", "OK");
                    
                    // Check if we've reached the last question
                    if (QuestionNumber < TotalQuestions)
                    {
                        // Navigate to next question - using direct page creation for safety
                        var parameters = new Dictionary<string, object>
                        {
                            { "Test", Test },
                            { "questionNumber", QuestionNumber + 1 },
                            { "totalQuestions", TotalQuestions },
                            { "IsEditMode", true }
                        };

                        // Clear current question data
                        QuestionText = string.Empty;
                        AnswerA = string.Empty;
                        AnswerB = string.Empty;
                        AnswerC = string.Empty;
                        AnswerD = string.Empty;
                        CorrectAnswer = string.Empty;
                        QuestionId = 0;
                        IsQuestionLoaded = false;
                        
                        try
                        {
                            // Create new page for next question (to avoid issues with reusing the same page)
                            var page = new MauiClientApp.Views.Tests.CreateQuestionPage();
                            
                            // Apply parameters manually
                            ((IQueryAttributable)page).ApplyQueryAttributes(parameters);
                            
                            // Use Application.Current.MainPage.Navigation directly
                            await Application.Current.MainPage.Navigation.PushAsync(page);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"CreateQuestionViewModel: Navigation error: {ex.Message}");
                            Console.WriteLine($"CreateQuestionViewModel: Stack trace: {ex.StackTrace}");
                            
                            // Fallback navigation
                            await Application.Current.MainPage.DisplayAlert("Warning", 
                                "Could not navigate to next question. You'll return to the tests list.", "OK");
                            await GoBackToTestsList();
                        }
                    }
                    else
                    {
                        // All questions completed
                        await Application.Current.MainPage.DisplayAlert("Success", "All questions updated successfully!", "OK");
                        await GoBackToTestsList();
                    }
                }
                else
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to update question", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateQuestionViewModel: Error updating question: {ex.Message}");
                Console.WriteLine($"CreateQuestionViewModel: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to update question: {ex.Message}", "OK");
            }
        }

        private async Task GoBackToTestsList()
        {
            try
            {
                Console.WriteLine("CreateQuestionViewModel: Navigating back to tests list");
                
                // Find the TestsPage in the navigation stack if it exists
                var navigationStack = Application.Current.MainPage.Navigation.NavigationStack;
                var testsPageIndex = navigationStack.ToList().FindIndex(p => p is Views.TestsPage);
                
                if (testsPageIndex >= 0)
                {
                    // If TestsPage is found in the stack, pop back to it
                    Console.WriteLine("CreateQuestionViewModel: TestsPage found in navigation stack, popping back to it");
                    
                    while (Application.Current.MainPage.Navigation.NavigationStack.Count > testsPageIndex + 1)
                    {
                        await Application.Current.MainPage.Navigation.PopAsync();
                    }
                }
                else
                {
                    // If TestsPage is not in the stack, try to navigate to DashboardPage
                    Console.WriteLine("CreateQuestionViewModel: TestsPage not found, checking for DashboardPage");
                    var dashboardPageIndex = navigationStack.ToList().FindIndex(p => p is Views.DashboardPage);
                    
                    if (dashboardPageIndex >= 0)
                    {
                        // Pop back to DashboardPage
                        Console.WriteLine("CreateQuestionViewModel: DashboardPage found, popping back to it");
                        
                        while (Application.Current.MainPage.Navigation.NavigationStack.Count > dashboardPageIndex + 1)
                        {
                            await Application.Current.MainPage.Navigation.PopAsync();
                        }
                    }
                    else
                    {
                        // If all else fails, push a new TestsPage
                        Console.WriteLine("CreateQuestionViewModel: No TestsPage or DashboardPage found, pushing new TestsPage");
                        await Application.Current.MainPage.Navigation.PopToRootAsync();
                        await Application.Current.MainPage.Navigation.PushAsync(new Views.TestsPage());
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateQuestionViewModel: Error in GoBackToTestsList: {ex.Message}");
                Console.WriteLine($"CreateQuestionViewModel: Stack trace: {ex.StackTrace}");
                
                // As a last resort, try to push a new TestsPage
                try
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new Views.TestsPage());
                }
                catch
                {
                    // Absolute last resort - just go to the dashboard without resetting app shell
                    var navigationPage = new NavigationPage(new Views.DashboardPage());
                    Application.Current.MainPage = navigationPage;
                }
            }
        }

        private async void OnBack()
        {
            try
            {
                bool shouldGoBack = await Application.Current.MainPage.DisplayAlert(
                    "Confirm", 
                    "Are you sure you want to go back? Any unsaved changes will be lost.", 
                    "Yes", "No");
                    
                if (shouldGoBack)
                {
                    if (Application.Current.MainPage.Navigation.NavigationStack.Count > 1)
                    {
                        await Application.Current.MainPage.Navigation.PopAsync();
                    }
                    else
                    {
                        // If we're somehow at the root of the navigation stack,
                        // just go back to the tests list
                        await GoBackToTestsList();
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateQuestionViewModel: Error in OnBack: {ex.Message}");
                Console.WriteLine($"CreateQuestionViewModel: Stack trace: {ex.StackTrace}");
                
                // If navigation fails, try to reset to app shell
                await Application.Current.MainPage.DisplayAlert("Error", 
                    "Navigation error. The app will return to the main page.", "OK");
                Application.Current.MainPage = new AppShell();
            }
        }

        private INavigation Navigation => Application.Current.MainPage.Navigation;

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 