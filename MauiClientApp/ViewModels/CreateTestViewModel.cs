using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;

namespace MauiClientApp.ViewModels
{
    public class CreateTestViewModel : INotifyPropertyChanged
    {
        private string _testTitle;
        private string _numberOfQuestions;
        private string _timeLimit;
        private bool _isNextEnabled;

        public string TestTitle
        {
            get => _testTitle;
            set
            {
                if (_testTitle != value)
                {
                    _testTitle = value;
                    OnPropertyChanged();
                    ValidateInputs();
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
                    ValidateInputs();
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

        public CreateTestViewModel()
        {
            NextCommand = new Command(OnNext, () => IsNextEnabled);
            BackCommand = new Command(OnBack);
            
            // Initialize with empty values
            TestTitle = string.Empty;
            NumberOfQuestions = string.Empty;
            TimeLimit = string.Empty;
            IsNextEnabled = false;
        }

        private void ValidateInputs()
        {
            // Validate that:
            // 1. Title is not empty
            // 2. NumberOfQuestions is a valid number > 0
            // 3. TimeLimit is a valid number > 0
            
            bool isValid = !string.IsNullOrWhiteSpace(TestTitle) &&
                           int.TryParse(NumberOfQuestions, out int questions) && questions > 0 &&
                           int.TryParse(TimeLimit, out int time) && time > 0;
            
            IsNextEnabled = isValid;
            ((Command)NextCommand).ChangeCanExecute();
        }

        private async void OnNext()
        {
            try
            {
                int questions = int.Parse(NumberOfQuestions);
                int timeInMinutes = int.Parse(TimeLimit);
                
                Test newTest = new Test
                {
                    TestName = TestTitle,
                    NumberOfQuestions = questions,
                    TimeLimit = timeInMinutes,
                    CompanyName = MauiClientApp.Services.SessionManager.Instance?.CompanyName
                };
                
                // Initialize collections
                newTest.Questions = new System.Collections.ObjectModel.ObservableCollection<Models.Question>();
                newTest.AssignedUsers = new System.Collections.ObjectModel.ObservableCollection<Models.User>();
                
                // Create a simple intermediate page to ensure the Test object is instantiated
                var questionPage = new MauiClientApp.Views.Tests.CreateQuestionPage();
                var viewModel = questionPage.BindingContext as CreateQuestionViewModel;
                
                if (viewModel != null)
                {
                    viewModel.QuestionNumber = 1;
                    viewModel.TotalQuestions = questions;
                    viewModel.Test = newTest;
                    
                    // Navigate using direct page navigation instead of Shell
                    await Application.Current.MainPage.Navigation.PushAsync(questionPage);
                }
                else
                {
                    // Fallback to Shell navigation
                    await Shell.Current.GoToAsync($"//CreateQuestionPage?questionNumber=1&totalQuestions={questions}",
                        new Dictionary<string, object>
                        {
                            { "Test", newTest }
                        });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnBack()
        {
            await Shell.Current.GoToAsync("..");
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 