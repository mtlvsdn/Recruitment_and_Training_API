using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;

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
                            test_name = "New Test",
                            no_of_questions = 0,
                            time_limit = 60,
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
                        
                        Console.WriteLine($"Test object details: Name={Test.test_name}, Questions={Test.Questions.Count}");
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

        private readonly ApiService _apiService;

        public TestSummaryViewModel()
        {
            _apiService = new ApiService();
            FinishCommand = new Command(OnFinish);
            BackCommand = new Command(OnBack);
            Users = new ObservableCollection<UserSelectionItem>();
            IsFinishEnabled = true;
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                
                // Get all users for the company
                var companyUsers = await _apiService.GetListAsync<User>("user");
                var filteredUsers = companyUsers.Where(u => u.Company_Name == Test.company_name).ToList();

                // Get all user-test assignments for this test
                var assignments = await _apiService.GetListAsync<UserTest>($"user-test/test/{Test.test_id}");

                var userItems = new ObservableCollection<UserSelectionItem>();
                foreach (var user in filteredUsers)
                {
                    userItems.Add(new UserSelectionItem
                    {
                        User = user,
                        IsSelected = assignments.Any(a => a.Userid == user.Id)
                    });
                }

                Users = userItems;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async void OnFinish()
        {
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
            
            try
            {
                // Get current assignments
                var currentAssignments = await _apiService.GetListAsync<UserTest>($"user-test/test/{Test.test_id}");
                var selectedUsers = Users.Where(u => u.IsSelected).Select(u => u.User.Id).ToList();

                // Remove assignments that are no longer selected
                foreach (var assignment in currentAssignments)
                {
                    if (!selectedUsers.Contains(assignment.Userid))
                    {
                        await _apiService.DeleteAsync($"user-test/{assignment.Userid}-{assignment.Testtest_id}");
                    }
                }

                // Add new assignments
                foreach (var userId in selectedUsers)
                {
                    if (!currentAssignments.Any(a => a.Userid == userId))
                    {
                        var newAssignment = new UserTest
                        {
                            Userid = userId,
                            Testtest_id = Test.test_id
                        };
                        await _apiService.PostAsync<UserTest>("user-test", newAssignment);
                    }
                }

                await Application.Current.MainPage.DisplayAlert("Success", "Test assignments updated successfully!", "OK");
                await Shell.Current.GoToAsync("//DashboardPage");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in OnFinish: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to complete operation: {ex.Message}", "OK");
            }
            finally
            {
                IsFinishEnabled = true;
            }
        }

        private async void OnBack()
        {
            try
            {
                await Shell.Current.GoToAsync("..");
            }
            catch (Exception ex)
            {
                await Shell.Current.DisplayAlert("Navigation Error", $"Failed to navigate back: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 