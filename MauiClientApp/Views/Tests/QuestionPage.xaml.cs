using MauiClientApp.Models;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Collections.ObjectModel;
using System.Linq;

namespace MauiClientApp.Views.Tests
{
    public partial class QuestionPage : ContentPage, INotifyPropertyChanged
    {
        private TestSession _testSession;
        private Question _currentQuestion;
        private string _selectedAnswer;
        private bool _isNextEnabled;
        private int _questionNumber;
        private int _totalQuestions;
        private string _testName;
        private string _timeRemaining;
        private System.Timers.Timer _timer;

        public event PropertyChangedEventHandler PropertyChanged;

        public Question CurrentQuestion
        {
            get => _currentQuestion;
            set
            {
                _currentQuestion = value;
                OnPropertyChanged();
            }
        }

        public string SelectedAnswer
        {
            get => _selectedAnswer;
            set
            {
                _selectedAnswer = value;
                IsNextEnabled = !string.IsNullOrEmpty(value); // Enable Next only when an answer is selected
                OnPropertyChanged();
            }
        }

        public bool IsNextEnabled
        {
            get => _isNextEnabled;
            set
            {
                _isNextEnabled = value;
                OnPropertyChanged();
            }
        }

        public int QuestionNumber
        {
            get => _questionNumber;
            set
            {
                _questionNumber = value;
                OnPropertyChanged();
            }
        }

        public int TotalQuestions
        {
            get => _totalQuestions;
            set
            {
                _totalQuestions = value;
                OnPropertyChanged();
            }
        }

        public string TestName
        {
            get => _testName;
            set
            {
                _testName = value;
                OnPropertyChanged();
            }
        }

        public string TimeRemaining
        {
            get => _timeRemaining;
            set
            {
                _timeRemaining = value;
                OnPropertyChanged();
            }
        }

        public QuestionPage(TestSession testSession)
        {
            InitializeComponent();
            _testSession = testSession;
            
            // Set the binding context to this instance
            BindingContext = this;
            
            // Initialize properties
            TestName = _testSession.Test.test_name;
            TotalQuestions = _testSession.Test.no_of_questions;
            LoadCurrentQuestion();
            
            // Start timer for updating remaining time
            _timer = new System.Timers.Timer(1000);
            _timer.Elapsed += (s, e) => MainThread.BeginInvokeOnMainThread(UpdateTimeRemaining);
            _timer.Start();
        }

        private void LoadCurrentQuestion()
        {
            if (_testSession.CurrentQuestionIndex < _testSession.Questions.Count)
            {
                CurrentQuestion = _testSession.Questions[_testSession.CurrentQuestionIndex];
                QuestionNumber = _testSession.CurrentQuestionIndex + 1;
                SelectedAnswer = string.Empty;
                IsNextEnabled = false;
                
                // Uncheck all radio buttons first
                RadioA.IsChecked = false;
                RadioB.IsChecked = false;
                RadioC.IsChecked = false;
                RadioD.IsChecked = false;
                
                // Check if we already have an answer for this question
                var existingAnswer = _testSession.UserAnswers.FirstOrDefault(a => a.QuestionId == CurrentQuestion.question_id);
                if (existingAnswer != null && existingAnswer.IsAnswered)
                {
                    SelectedAnswer = existingAnswer.SelectedAnswer;
                    IsNextEnabled = true;
                    
                    // Set the appropriate radio button checked
                    if (SelectedAnswer == CurrentQuestion.possible_answer_1)
                        RadioA.IsChecked = true;
                    else if (SelectedAnswer == CurrentQuestion.possible_answer_2)
                        RadioB.IsChecked = true;
                    else if (SelectedAnswer == CurrentQuestion.possible_answer_3)
                        RadioC.IsChecked = true;
                    else if (SelectedAnswer == CurrentQuestion.possible_answer_4)
                        RadioD.IsChecked = true;
                }
            }
            else
            {
                // No more questions, complete the test
                CompleteTest();
            }
        }

        private void UpdateTimeRemaining()
        {
            var remaining = _testSession.RemainingTime;
            if (remaining.TotalSeconds <= 0)
            {
                // Time's up, complete the test
                _timer.Stop();
                MainThread.BeginInvokeOnMainThread(CompleteTest);
                return;
            }
            
            TimeRemaining = $"{remaining.Minutes:D2}:{remaining.Seconds:D2}";
        }

        private void OnAnswerSelected(object sender, EventArgs e)
        {
            if (sender is RadioButton radioButton && radioButton.IsChecked)
            {
                string answerText = "";
                
                // Map the radio button value (A, B, C, D) to the actual answer text
                switch (radioButton.Value?.ToString())
                {
                    case "A":
                        answerText = CurrentQuestion.possible_answer_1;
                        break;
                    case "B":
                        answerText = CurrentQuestion.possible_answer_2;
                        break;
                    case "C":
                        answerText = CurrentQuestion.possible_answer_3;
                        break;
                    case "D":
                        answerText = CurrentQuestion.possible_answer_4;
                        break;
                }
                
                SelectedAnswer = answerText;
                Console.WriteLine($"QuestionPage: Selected answer: {radioButton.Value} - {SelectedAnswer}");
            }
        }

        private async void OnNextClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(SelectedAnswer))
            {
                await DisplayAlert("Warning", "Please select an answer before proceeding.", "OK");
                return;
            }
            
            // Save the answer
            SaveCurrentAnswer();
            
            // Move to the next question or complete the test
            _testSession.CurrentQuestionIndex++;
            
            if (_testSession.CurrentQuestionIndex < _testSession.Questions.Count)
            {
                // Load the next question
                LoadCurrentQuestion();
            }
            else
            {
                // No more questions, complete the test
                CompleteTest();
            }
        }

        private void SaveCurrentAnswer()
        {
            // Check if we already have an answer for this question
            var existingAnswer = _testSession.UserAnswers.FirstOrDefault(a => a.QuestionId == CurrentQuestion.question_id);
            
            // Create a new answer or update existing one
            if (existingAnswer == null)
            {
                var userAnswer = new UserAnswer
                {
                    QuestionId = CurrentQuestion.question_id,
                    SelectedAnswer = SelectedAnswer,
                    IsCorrect = SelectedAnswer == CurrentQuestion.correct_answer,
                    IsAnswered = true,
                    Question = CurrentQuestion
                };
                
                _testSession.UserAnswers.Add(userAnswer);
                
                if (userAnswer.IsCorrect)
                {
                    _testSession.CorrectAnswers++;
                }
            }
            else
            {
                // Update existing answer
                if (existingAnswer.IsCorrect)
                {
                    _testSession.CorrectAnswers--;
                }
                
                existingAnswer.SelectedAnswer = SelectedAnswer;
                existingAnswer.IsCorrect = SelectedAnswer == CurrentQuestion.correct_answer;
                existingAnswer.IsAnswered = true;
                
                if (existingAnswer.IsCorrect)
                {
                    _testSession.CorrectAnswers++;
                }
            }
        }

        private async void CompleteTest()
        {
            // Stop the timer
            _timer?.Stop();
            
            // Set the end time and mark as completed
            _testSession.EndTime = DateTime.Now;
            _testSession.IsCompleted = true;
            
            // Navigate to the results page
            try
            {
                var resultsPage = new TestResultsPage(_testSession);
                await Navigation.PushAsync(resultsPage);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"QuestionPage: Error navigating to results page: {ex.Message}");
                await DisplayAlert("Error", "There was an error displaying the results. Your test has been completed.", "OK");
                await Navigation.PopToRootAsync();
            }
        }

        protected override void OnDisappearing()
        {
            base.OnDisappearing();
            _timer?.Stop();
            _timer?.Dispose();
        }

        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 