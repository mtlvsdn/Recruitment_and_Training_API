using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;

namespace MauiClientApp.ViewModels
{
    [QueryProperty(nameof(QuestionNumber), "questionNumber")]
    [QueryProperty(nameof(TotalQuestions), "totalQuestions")]
    [QueryProperty(nameof(Test), "Test")]
    public class CreateQuestionViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private int _questionNumber;
        private int _totalQuestions;
        private Test _test;
        private string _questionText;
        private string _answerA;
        private string _answerB;
        private string _answerC;
        private string _answerD;
        private bool _isNextEnabled;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            if (query.ContainsKey("questionNumber") && int.TryParse(query["questionNumber"].ToString(), out int questionNumber))
            {
                QuestionNumber = questionNumber;
            }
            
            if (query.ContainsKey("totalQuestions") && int.TryParse(query["totalQuestions"].ToString(), out int totalQuestions))
            {
                TotalQuestions = totalQuestions;
            }
            
            if (query.ContainsKey("Test"))
            {
                Test = query["Test"] as Test;
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

        public bool IsNextEnabled
        {
            get => _isNextEnabled;
            set
            {
                if (_isNextEnabled != value)
                {
                    _isNextEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }

        public CreateQuestionViewModel()
        {
            NextCommand = new Command(OnNext, () => IsNextEnabled);
            BackCommand = new Command(OnBack);
            
            // Initialize with empty values
            QuestionText = string.Empty;
            AnswerA = string.Empty;
            AnswerB = string.Empty;
            AnswerC = string.Empty;
            AnswerD = string.Empty;
            IsNextEnabled = false;
        }

        private void ValidateInputs()
        {
            bool isValid = !string.IsNullOrWhiteSpace(QuestionText) &&
                          !string.IsNullOrWhiteSpace(AnswerA) &&
                          !string.IsNullOrWhiteSpace(AnswerB) &&
                          !string.IsNullOrWhiteSpace(AnswerC) &&
                          !string.IsNullOrWhiteSpace(AnswerD);
            
            IsNextEnabled = isValid;
            ((Command)NextCommand).ChangeCanExecute();
        }

        private async void OnNext()
        {
            try
            {
                // Create question object
                Question question = new Question
                {
                    QuestionText = QuestionText,
                    PossibleAnswer1 = AnswerA,
                    PossibleAnswer2 = AnswerB,
                    PossibleAnswer3 = AnswerC,
                    PossibleAnswer4 = AnswerD,
                    // We'll set the correct answer as Answer A for now, but this could be enhanced with a radio button selection
                    CorrectAnswer = AnswerA
                };
                
                // Safety check for null Test
                if (Test == null)
                {
                    Test = new Test
                    {
                        TestName = "New Test",
                        NumberOfQuestions = TotalQuestions,
                        TimeLimit = 60 // Default 60 minutes
                    };
                }
                
                // Always ensure NumberOfQuestions is correctly set
                Test.NumberOfQuestions = TotalQuestions;
                
                // Safety check for null Questions collection
                if (Test.Questions == null)
                {
                    Test.Questions = new System.Collections.ObjectModel.ObservableCollection<Question>();
                }
                
                // Add the question to the test
                Test.Questions.Add(question);
                
                if (QuestionNumber < TotalQuestions)
                {
                    try
                    {
                        // Navigate to the next question using direct page creation
                        var nextPage = new MauiClientApp.Views.Tests.CreateQuestionPage();
                        var nextViewModel = nextPage.BindingContext as CreateQuestionViewModel;
                        
                        if (nextViewModel != null)
                        {
                            // Directly set properties instead of using query parameters
                            nextViewModel.QuestionNumber = QuestionNumber + 1;
                            nextViewModel.TotalQuestions = TotalQuestions;
                            nextViewModel.Test = Test;
                            
                            await Application.Current.MainPage.Navigation.PushAsync(nextPage);
                        }
                        else
                        {
                            // Fallback to Shell navigation
                            int nextQuestionNumber = QuestionNumber + 1;
                            await Shell.Current.GoToAsync($"//CreateQuestionPage?questionNumber={nextQuestionNumber}&totalQuestions={TotalQuestions}",
                                new Dictionary<string, object>
                                {
                                    { "Test", Test }
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        await Shell.Current.DisplayAlert("Navigation Error", $"Failed to navigate to next question: {ex.Message}", "OK");
                    }
                }
                else
                {
                    try
                    {
                        // Navigate to summary page using direct page creation
                        var summaryPage = new MauiClientApp.Views.Tests.TestSummaryPage();
                        if (summaryPage.BindingContext is TestSummaryViewModel summaryViewModel)
                        {
                            // Make sure Test is not null before passing it
                            if (Test == null)
                            {
                                Test = new Test
                                {
                                    TestName = "New Test",
                                    NumberOfQuestions = TotalQuestions,
                                    TimeLimit = 60, // Default 60 minutes
                                    Questions = new System.Collections.ObjectModel.ObservableCollection<Question>()
                                };
                            }

                            // Make sure Questions collection is never null
                            if (Test.Questions == null)
                            {
                                Test.Questions = new System.Collections.ObjectModel.ObservableCollection<Question>();
                            }

                            // Add the current question to the test if it doesn't already contain it
                            Question currentQuestion = new Question
                            {
                                QuestionText = QuestionText,
                                PossibleAnswer1 = AnswerA,
                                PossibleAnswer2 = AnswerB,
                                PossibleAnswer3 = AnswerC,
                                PossibleAnswer4 = AnswerD,
                                CorrectAnswer = AnswerA // You might want to improve this
                            };

                            // Avoid duplicate questions
                            bool questionExists = Test.Questions.Any(q => q.QuestionText == currentQuestion.QuestionText);
                            if (!questionExists)
                            {
                                Test.Questions.Add(currentQuestion);
                            }

                            // Set Test properties and navigate
                            summaryViewModel.Test = Test;
                            await Application.Current.MainPage.Navigation.PushAsync(summaryPage);
                        }
                        else
                        {
                            // Enhanced logging for debugging
                            Console.WriteLine("WARNING: summaryViewModel is null!");

                            // Fallback to Shell navigation with additional safety checks
                            if (Test == null)
                            {
                                // Create a minimal valid Test object for navigation
                                Test = new Test
                                {
                                    TestName = "New Test",
                                    NumberOfQuestions = TotalQuestions,
                                    TimeLimit = 60
                                };
                                Console.WriteLine("Created default Test object for navigation");
                            }

                            await Shell.Current.GoToAsync("//TestSummaryPage",
                                new Dictionary<string, object>
                                {
                                    { "Test", Test }
                                });
                        }
                    }
                    catch (Exception ex)
                    {
                        // Enhanced error logging
                        Console.WriteLine($"Navigation Error Details: {ex.GetType().Name}");
                        Console.WriteLine($"Stack Trace: {ex.StackTrace}");
                        await Shell.Current.DisplayAlert("Navigation Error", $"Failed to navigate to summary page: {ex.Message}", "OK");
                    }
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnBack()
        {
            if (QuestionNumber > 1)
            {
                // Go back to the previous question
                int prevQuestionNumber = QuestionNumber - 1;
                await Shell.Current.GoToAsync($"//CreateQuestionPage?questionNumber={prevQuestionNumber}&totalQuestions={TotalQuestions}",
                    new Dictionary<string, object>
                    {
                        { "Test", Test }
                    });
            }
            else
            {
                // Go back to test creation page
                await Shell.Current.GoToAsync("//CreateTestPage");
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        public virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 