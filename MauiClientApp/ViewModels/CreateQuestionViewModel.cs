using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;
using MauiClientApp.Views.Tests;

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
        private string _correctAnswer;

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
                }
            }
        }

        public ICommand NextCommand { get; }
        public ICommand BackCommand { get; }

        public CreateQuestionViewModel()
        {
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
        }

        private void ValidateInputs()
        {
            bool isValid = !string.IsNullOrWhiteSpace(QuestionText) &&
                          !string.IsNullOrWhiteSpace(AnswerA) &&
                          !string.IsNullOrWhiteSpace(AnswerB) &&
                          !string.IsNullOrWhiteSpace(AnswerC) &&
                          !string.IsNullOrWhiteSpace(AnswerD) &&
                          !string.IsNullOrWhiteSpace(CorrectAnswer) &&
                          (CorrectAnswer == AnswerA || 
                           CorrectAnswer == AnswerB || 
                           CorrectAnswer == AnswerC || 
                           CorrectAnswer == AnswerD);
            
            IsNextEnabled = isValid;
            ((Command)NextCommand).ChangeCanExecute();
        }

        private async Task OnNext()
        {
            if (string.IsNullOrWhiteSpace(QuestionText))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", "Please enter a question", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(AnswerA) || string.IsNullOrWhiteSpace(AnswerB) ||
                string.IsNullOrWhiteSpace(AnswerC) || string.IsNullOrWhiteSpace(AnswerD))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", "Please fill in all answer options", "OK");
                return;
            }

            if (string.IsNullOrWhiteSpace(CorrectAnswer))
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", "Please enter the correct answer", "OK");
                return;
            }

            // Check if the correct answer matches one of the possible answers
            if (CorrectAnswer != AnswerA && CorrectAnswer != AnswerB && 
                CorrectAnswer != AnswerC && CorrectAnswer != AnswerD)
            {
                await Application.Current.MainPage.DisplayAlert("Validation Error", 
                    "The correct answer must exactly match one of the possible answers (A, B, C, or D)", "OK");
                return;
            }

            // Create the question object
            var question = new Question
            {
                QuestionText = QuestionText,
                PossibleAnswer1 = AnswerA,
                PossibleAnswer2 = AnswerB,
                PossibleAnswer3 = AnswerC,
                PossibleAnswer4 = AnswerD,
                CorrectAnswer = CorrectAnswer
            };

            // Add the question to the test
            Test.Questions.Add(question);

            // If this was the last question, navigate to the test summary
            if (QuestionNumber >= TotalQuestions)
            {
                var summaryPage = new TestSummaryPage();
                var viewModel = summaryPage.BindingContext as TestSummaryViewModel;
                if (viewModel != null)
                {
                    viewModel.Test = Test;
                }
                await Application.Current.MainPage.Navigation.PushAsync(summaryPage);
            }
            else
            {
                // Clear the form for the next question
                QuestionNumber++;
                QuestionText = string.Empty;
                AnswerA = string.Empty;
                AnswerB = string.Empty;
                AnswerC = string.Empty;
                AnswerD = string.Empty;
                CorrectAnswer = string.Empty;
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