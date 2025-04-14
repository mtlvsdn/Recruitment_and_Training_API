using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;

namespace MauiClientApp.ViewModels
{
    public class CreateTestViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private string _testTitle;
        private string _numberOfQuestions;
        private string _timeLimit;
        private bool _isNextEnabled;
        private bool _isSaving;

        public bool IsSaving
        {
            get => _isSaving;
            set
            {
                if (_isSaving != value)
                {
                    _isSaving = value;
                    OnPropertyChanged();
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
            _apiService = new ApiService();
            BackCommand = new Command(async () => await OnBack());
            NextCommand = new Command(async () => await OnNext(), () => IsNextEnabled && !IsSaving);
            
            // Initialize with empty values
            TestTitle = string.Empty;
            NumberOfQuestions = string.Empty;
            TimeLimit = string.Empty;
            IsNextEnabled = false;
            IsSaving = false;
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

        private async Task OnNext()
        {
            if (IsSaving)
                return;

            try
            {
                IsSaving = true;
                ((Command)NextCommand).ChangeCanExecute();

                int questions = int.Parse(NumberOfQuestions);
                int timeInMinutes = int.Parse(TimeLimit);
                
                // Create test model to save to database
                var testToSave = new Test
                {
                    test_name = TestTitle,
                    no_of_questions = questions,
                    time_limit = timeInMinutes,
                    company_name = MauiClientApp.Services.SessionManager.Instance?.CompanyName
                };
                
                // Save the test to the database first
                Console.WriteLine($"Saving test to database: {testToSave.test_name}");
                var savedTest = await _apiService.PostAsync<Test>("test", testToSave);
                
                if (savedTest == null || savedTest.test_id <= 0)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", 
                        "Failed to save the test. Please try again.", 
                        "OK");
                    return;
                }
                
                Console.WriteLine($"Test saved successfully with ID: {savedTest.test_id}");
                
                // Now create the full test object for the UI with the ID from the server
                Test newTest = new Test
                {
                    test_id = savedTest.test_id,
                    TestName = savedTest.test_name,
                    NumberOfQuestions = savedTest.no_of_questions,
                    TimeLimit = savedTest.time_limit,
                    CompanyName = savedTest.company_name
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
                await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                Console.WriteLine($"CreateTestViewModel: Error in OnNext: {ex.Message}");
                Console.WriteLine($"CreateTestViewModel: Stack trace: {ex.StackTrace}");
            }
            finally
            {
                IsSaving = false;
                ((Command)NextCommand).ChangeCanExecute();
            }
        }

        private async Task OnBack()
        {
            try
            {
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Navigation Error", $"Failed to navigate back: {ex.Message}", "OK");
            }
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