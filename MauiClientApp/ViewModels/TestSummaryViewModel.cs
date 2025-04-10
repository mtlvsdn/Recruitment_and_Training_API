using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;

namespace MauiClientApp.ViewModels
{
    [QueryProperty(nameof(Test), "Test")]
    public class TestSummaryViewModel : INotifyPropertyChanged, IQueryAttributable
    {
        private Test _test;
        private ObservableCollection<UserSelectionItem> _users;
        private bool _isLoading;
        private bool _isFinishEnabled;

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            try
            {
                Console.WriteLine("TestSummaryViewModel.ApplyQueryAttributes called");
                
                if (query.ContainsKey("Test"))
                {
                    var testObject = query["Test"];
                    Console.WriteLine($"Test object received: {testObject != null}");
                    
                    Test = testObject as Test;
                    
                    // Additional safeguard for Test object
                    if (Test == null)
                    {
                        Console.WriteLine("WARNING: Received null Test object, creating a new one");
                        Test = new Test
                        {
                            TestName = "New Test",
                            NumberOfQuestions = 0,
                            TimeLimit = 60,
                            Questions = new ObservableCollection<Question>(),
                            AssignedUsers = new ObservableCollection<User>()
                        };
                    }
                    else
                    {
                        // Ensure Collections are not null
                        if (Test.Questions == null)
                        {
                            Test.Questions = new ObservableCollection<Question>();
                        }
                        
                        if (Test.AssignedUsers == null)
                        {
                            Test.AssignedUsers = new ObservableCollection<User>();
                        }
                        
                        Console.WriteLine($"Test object details: Name={Test.TestName}, Questions={Test.Questions.Count}");
                    }
                    
                    // Load users when test is set - safely
                    Task.Run(async () => 
                    {
                        try
                        {
                            await LoadUsersAsync();
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"Error loading users: {ex.Message}");
                        }
                    });
                }
                else
                {
                    Console.WriteLine("No Test object in query parameters");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in ApplyQueryAttributes: {ex.Message}");
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

        public ObservableCollection<UserSelectionItem> Users
        {
            get => _users;
            set
            {
                if (_users != value)
                {
                    _users = value;
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

        public bool IsFinishEnabled
        {
            get => _isFinishEnabled;
            set
            {
                if (_isFinishEnabled != value)
                {
                    _isFinishEnabled = value;
                    OnPropertyChanged();
                }
            }
        }

        public ICommand FinishCommand { get; }
        public ICommand BackCommand { get; }

        public TestSummaryViewModel()
        {
            FinishCommand = new Command(OnFinish);
            BackCommand = new Command(OnBack);
            Users = new ObservableCollection<UserSelectionItem>();
            IsFinishEnabled = true;
        }

        // Helper method to get diagnostic info
        private string GetDiagnosticInfo()
        {
            var info = new System.Text.StringBuilder();
            
            info.AppendLine($"Test is null: {Test == null}");
            
            if (Test != null)
            {
                info.AppendLine($"Test.TestId: {Test.TestId}");
                info.AppendLine($"Test.TestName: {Test.TestName}");
                info.AppendLine($"Test.NumberOfQuestions: {Test.NumberOfQuestions}");
                info.AppendLine($"Test.TimeLimit: {Test.TimeLimit}");
                info.AppendLine($"Test.CompanyName: {Test.CompanyName ?? "null"}");
                info.AppendLine($"Test.Questions is null: {Test.Questions == null}");
                info.AppendLine($"Test.AssignedUsers is null: {Test.AssignedUsers == null}");
            }
            
            info.AppendLine($"Users is null: {Users == null}");
            
            if (Users != null)
            {
                info.AppendLine($"Users count: {Users.Count}");
                info.AppendLine($"Selected users count: {Users.Count(u => u.IsSelected)}");
            }
            
            info.AppendLine($"SessionManager.Instance is null: {MauiClientApp.Services.SessionManager.Instance == null}");
            
            if (MauiClientApp.Services.SessionManager.Instance != null)
            {
                info.AppendLine($"SessionManager.CompanyName: {MauiClientApp.Services.SessionManager.Instance.CompanyName ?? "null"}");
            }
            
            return info.ToString();
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                Console.WriteLine("Loading users...");
                
                // Get the company name from the session manager
                var companyName = MauiClientApp.Services.SessionManager.Instance.CompanyName;
                
                Console.WriteLine($"Company name from session: {companyName}");
                
                if (string.IsNullOrEmpty(companyName))
                {
                    Console.WriteLine("No company name found in session");
                    await Application.Current.MainPage.DisplayAlert("Error", "Company name not found in session", "OK");
                    return;
                }
                
                // Create an API service instance
                var apiService = new MauiClientApp.Services.ApiService();
                
                // Clear any existing users
                Users.Clear();
                
                // Fetch all users from the API
                var allUsers = await apiService.GetListAsync<User>("user");
                
                if (allUsers != null)
                {
                    // Filter users by company name
                    var companyUsers = allUsers.Where(u => u.Company_Name == companyName).ToList();
                    
                    Console.WriteLine($"Found {companyUsers.Count} users for company {companyName}");
                    
                    if (companyUsers.Count > 0)
                    {
                        foreach (var user in companyUsers)
                        {
                            Console.WriteLine($"Adding user: {user.Full_Name}");
                            Users.Add(new UserSelectionItem
                            {
                                User = user,
                                IsSelected = false
                            });
                        }
                        
                        Console.WriteLine($"Users collection now has {Users.Count} items");
                    }
                    else
                    {
                        Console.WriteLine("No users found for this company");
                        await Application.Current.MainPage.DisplayAlert("Warning", "No users found for your company", "OK");
                    }
                }
                else
                {
                    Console.WriteLine("Failed to fetch users from API");
                    await Application.Current.MainPage.DisplayAlert("Error", "Failed to fetch users from the server", "OK");
                }
            }
            catch (Exception ex)
            {
                // Handle error
                Console.WriteLine($"Error in LoadUsersAsync: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await Shell.Current.DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnFinish()
        {
            try
            {
                // Simple validation
                if (Users == null || Test == null)
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Missing test data or users", "OK");
                    return;
                }
                
                if (!Users.Any(u => u.IsSelected))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Please select at least one user to take the test.", "OK");
                    return;
                }
                
                IsFinishEnabled = false;
                
                // Get the API service
                var apiService = new MauiClientApp.Services.ApiService();
                
                try
                {
                    // Create a properly formatted test object for the API
                    var apiTest = new 
                    {
                        test_name = Test.TestName ?? "Test",
                        no_of_questions = Test.NumberOfQuestions,
                        time_limit = Test.TimeLimit
                    };
                    
                    Console.WriteLine($"Saving test: {System.Text.Json.JsonSerializer.Serialize(apiTest)}");
                    
                    // Save the test using the API endpoint
                    var savedTest = await apiService.PostAsync<dynamic>("test", apiTest);
                    
                    if (savedTest != null)
                    {
                        int testId = Convert.ToInt32(savedTest.test_id);
                        Console.WriteLine($"Test saved with ID: {testId}");
                        
                        // Save each question
                        foreach (var question in Test.Questions)
                        {
                            try
                            {
                                // Create a properly formatted question object for the API
                                var apiQuestion = new
                                {
                                    test_id = testId,
                                    question_text = question.QuestionText,
                                    possible_answer_1 = question.PossibleAnswer1,
                                    possible_answer_2 = question.PossibleAnswer2,
                                    possible_answer_3 = question.PossibleAnswer3,
                                    possible_answer_4 = question.PossibleAnswer4,
                                    correct_answer = question.CorrectAnswer
                                };
                                
                                Console.WriteLine($"Saving question: {apiQuestion.question_text}");
                                var savedQuestion = await apiService.PostAsync<dynamic>("questions", apiQuestion);
                                Console.WriteLine("Question saved successfully");
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error saving question: {ex.Message}");
                            }
                        }
                        
                        // Associate test with selected users using User_Test junction table
                        foreach (var userItem in Users.Where(u => u.IsSelected))
                        {
                            try
                            {
                                if (userItem?.User != null)
                                {
                                    // Create a properly formatted User_Test association
                                    var userTestAssociation = new
                                    {
                                        Userid = userItem.User.Id,
                                        Testtest_id = testId
                                    };
                                    
                                    Console.WriteLine($"Associating test with user: {userItem.User.Full_Name}");
                                    var association = await apiService.PostAsync<dynamic>("user-test", userTestAssociation);
                                    Console.WriteLine("Test associated with user successfully");
                                }
                            }
                            catch (Exception ex)
                            {
                                Console.WriteLine($"Error associating test with user: {ex.Message}");
                            }
                        }
                        
                        await Application.Current.MainPage.DisplayAlert("Success", "Test created successfully and assigned to users!", "OK");
                        
                        // Navigate back to company dashboard
                        await Shell.Current.GoToAsync("//DashboardPage");
                    }
                    else
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", "Failed to save test - no response received", "OK");
                    }
                }
                catch (Exception apiEx)
                {
                    Console.WriteLine($"API Error: {apiEx.Message}");
                    Console.WriteLine($"Stack trace: {apiEx.StackTrace}");
                    await Application.Current.MainPage.DisplayAlert("API Error", $"Failed to save test: {apiEx.Message}", "OK");
                }
                
                IsFinishEnabled = true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Unexpected error: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"An unexpected error occurred: {ex.Message}", "OK");
                IsFinishEnabled = true;
            }
        }

        private async void OnBack()
        {
            try
            {
                // Go back to the last question page using direct navigation
                var questionPage = new MauiClientApp.Views.Tests.CreateQuestionPage();
                var viewModel = questionPage.BindingContext as CreateQuestionViewModel;
                
                if (viewModel != null && Test != null)
                {
                    viewModel.QuestionNumber = Test.NumberOfQuestions;
                    viewModel.TotalQuestions = Test.NumberOfQuestions;
                    viewModel.Test = Test;
                    
                    await Application.Current.MainPage.Navigation.PushAsync(questionPage);
                }
                else
                {
                    // Fallback to Shell navigation
                    await Shell.Current.GoToAsync($"//CreateQuestionPage?questionNumber={Test?.NumberOfQuestions ?? 1}&totalQuestions={Test?.NumberOfQuestions ?? 1}",
                        new Dictionary<string, object>
                        {
                            { "Test", Test }
                        });
                }
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Navigation Error", $"Failed to navigate back: {ex.Message}", "OK");
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

    public class UserSelectionItem : INotifyPropertyChanged
    {
        private User _user;
        private bool _isSelected;

        public User User
        {
            get => _user;
            set
            {
                if (_user != value)
                {
                    _user = value;
                    OnPropertyChanged();
                }
            }
        }

        public bool IsSelected
        {
            get => _isSelected;
            set
            {
                if (_isSelected != value)
                {
                    _isSelected = value;
                    OnPropertyChanged();
                }
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