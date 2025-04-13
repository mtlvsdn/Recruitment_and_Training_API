using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;

namespace MauiClientApp.ViewModels
{
    public class AssignUsersViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private Test _test;
        private bool _isLoading;
        private string _testName;
        private string _testQuestions;
        private string _testTimeLimit;
        private string _testCompany;
        private ObservableCollection<UserSelectionItem> _users;

        public string TestName
        {
            get => _testName;
            set
            {
                _testName = value;
                OnPropertyChanged();
            }
        }

        public string TestQuestions
        {
            get => _testQuestions;
            set
            {
                _testQuestions = value;
                OnPropertyChanged();
            }
        }

        public string TestTimeLimit
        {
            get => _testTimeLimit;
            set
            {
                _testTimeLimit = value;
                OnPropertyChanged();
            }
        }

        public string TestCompany
        {
            get => _testCompany;
            set
            {
                _testCompany = value;
                OnPropertyChanged();
            }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set
            {
                _isLoading = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<UserSelectionItem> Users
        {
            get => _users;
            set
            {
                _users = value;
                OnPropertyChanged();
            }
        }

        public ICommand LoadUsersCommand { get; }
        public ICommand SaveCommand { get; }
        public ICommand RefreshCommand { get; }

        public AssignUsersViewModel()
        {
            _apiService = new ApiService();
            Users = new ObservableCollection<UserSelectionItem>();
            LoadUsersCommand = new Command(async () => await LoadUsersAsync());
            SaveCommand = new Command(async () => await SaveChangesAsync());
            RefreshCommand = new Command(async () => await LoadUsersAsync());
        }

        public void Initialize(Test test)
        {
            _test = test;
            TestName = $"Test: {test.test_name}";
            TestQuestions = test.no_of_questions.ToString();
            TestTimeLimit = $"{test.time_limit} minutes";
            TestCompany = test.company_name;
            
            LoadUsersCommand.Execute(null);
        }

        private async Task LoadUsersAsync()
        {
            try
            {
                Console.WriteLine("AssignUsersViewModel: Starting LoadUsersAsync");
                IsLoading = true;

                // Get all users for the company
                Console.WriteLine("AssignUsersViewModel: Fetching company users");
                var companyUsers = await _apiService.GetListAsync<User>("user");
                Console.WriteLine($"AssignUsersViewModel: Retrieved {companyUsers?.Count ?? 0} users");

                var filteredUsers = companyUsers.Where(u => u.Company_Name == _test.company_name).ToList();
                Console.WriteLine($"AssignUsersViewModel: Filtered to {filteredUsers.Count} users for company {_test.company_name}");

                // Get all user-test assignments for this test
                Console.WriteLine($"AssignUsersViewModel: Fetching assignments for test {_test.test_id}");
                var assignments = await _apiService.GetListAsync<UserTest>($"user-test/test/{_test.test_id}");
                Console.WriteLine($"AssignUsersViewModel: Retrieved {assignments?.Count ?? 0} assignments");

                var userItems = new ObservableCollection<UserSelectionItem>();
                foreach (var user in filteredUsers)
                {
                    userItems.Add(new UserSelectionItem
                    {
                        User = user,
                        IsSelected = assignments.Any(a => a.Userid == user.Id)
                    });
                }

                Console.WriteLine($"AssignUsersViewModel: Created {userItems.Count} user selection items");
                Users = userItems;
                Console.WriteLine("AssignUsersViewModel: LoadUsersAsync completed successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignUsersViewModel: Error in LoadUsersAsync: {ex.Message}");
                Console.WriteLine($"AssignUsersViewModel: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task SaveChangesAsync()
        {
            try
            {
                IsLoading = true;
                Console.WriteLine($"AssignUsersViewModel: Starting to save changes for test ID: {_test.test_id}");

                // Get current assignments
                var currentAssignments = await _apiService.GetListAsync<UserTest>($"user-test/test/{_test.test_id}");
                Console.WriteLine($"AssignUsersViewModel: Found {currentAssignments?.Count ?? 0} existing assignments");
                
                var selectedUsers = Users.Where(u => u.IsSelected).Select(u => u.User.Id).ToList();
                Console.WriteLine($"AssignUsersViewModel: User selected {selectedUsers.Count} users for assignment");

                int removedCount = 0;
                int addedCount = 0;

                // Remove assignments that are no longer selected
                if (currentAssignments != null)
                {
                    foreach (var assignment in currentAssignments)
                    {
                        if (!selectedUsers.Contains(assignment.Userid))
                        {
                            Console.WriteLine($"AssignUsersViewModel: Removing assignment for user ID: {assignment.Userid}");
                            bool success = await _apiService.DeleteAsync($"user-test/{assignment.Userid}-{assignment.Testtest_id}");
                            if (success)
                            {
                                removedCount++;
                                Console.WriteLine($"AssignUsersViewModel: Successfully removed assignment for user ID: {assignment.Userid}");
                            }
                            else
                            {
                                Console.WriteLine($"AssignUsersViewModel: Failed to remove assignment for user ID: {assignment.Userid}");
                            }
                        }
                    }
                }

                // Add new assignments
                foreach (var userId in selectedUsers)
                {
                    if (currentAssignments == null || !currentAssignments.Any(a => a.Userid == userId))
                    {
                        Console.WriteLine($"AssignUsersViewModel: Adding new assignment for user ID: {userId}");
                        var newAssignment = new UserTest
                        {
                            Userid = userId,
                            Testtest_id = _test.test_id
                        };
                        
                        var result = await _apiService.PostAsync<UserTest>("user-test", newAssignment);
                        if (result != null)
                        {
                            addedCount++;
                            Console.WriteLine($"AssignUsersViewModel: Successfully added assignment for user ID: {userId}");
                        }
                        else
                        {
                            Console.WriteLine($"AssignUsersViewModel: Failed to add assignment for user ID: {userId}");
                        }
                    }
                }

                Console.WriteLine($"AssignUsersViewModel: Completed saving changes. Removed: {removedCount}, Added: {addedCount}");
                await Application.Current.MainPage.DisplayAlert("Success", 
                    $"Changes saved successfully.\nAssigned users: {selectedUsers.Count}\nRemoved: {removedCount}, Added: {addedCount}", 
                    "OK");
                
                await Application.Current.MainPage.Navigation.PopAsync();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignUsersViewModel: Error in SaveChangesAsync: {ex.Message}");
                Console.WriteLine($"AssignUsersViewModel: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to save changes: {ex.Message}", "OK");
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