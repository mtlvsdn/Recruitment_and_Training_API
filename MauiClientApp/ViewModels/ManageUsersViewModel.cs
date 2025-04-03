using MauiClientApp.Models;
using MauiClientApp.Services;
using MauiClientApp.Views.Company;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class ManageUsersViewModel : INotifyPropertyChanged
    {
        private ObservableCollection<UserModel> _users;
        private readonly ApiService _apiService;
        private bool _isLoading;
        private string _errorMessage;

        public ObservableCollection<UserModel> Users
        {
            get => _users;
            set { _users = value; OnPropertyChanged(); }
        }

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        public string ErrorMessage
        {
            get => _errorMessage;
            set { _errorMessage = value; OnPropertyChanged(); }
        }

        public ICommand CreateNewUserCommand { get; }
        public ICommand EditUserCommand { get; }
        public ICommand DeleteUserCommand { get; }

        public ManageUsersViewModel()
        {
            _apiService = new ApiService();
            Users = new ObservableCollection<UserModel>();

            CreateNewUserCommand = new Command(async () => await CreateNewUser());
            EditUserCommand = new Command<UserModel>(async (user) =>
            {
                // If using Shell navigation with query parameters, you can register and pass the user ID.
                // Here we use a simple fallback:
                await Application.Current.MainPage.Navigation.PushAsync(new EditUserPage(user));
            });

            DeleteUserCommand = new Command<UserModel>(async (user) => await DeleteUser(user));
        }

        public async Task LoadUsersAsync()
        {
            try
            {
                IsLoading = true;
                ErrorMessage = string.Empty;

                // Get the company name from the session (set during company login)
                string companyName = SessionManager.Instance.CompanyName;

                // Retrieve all users from the API
                var allUsers = await _apiService.GetListAsync<UserModel>("user");

                // Filter users by company name if a company is logged in
                var companyUsers = string.IsNullOrEmpty(companyName)
                    ? allUsers
                    : allUsers.Where(u => u.Company_Name.Equals(companyName, StringComparison.OrdinalIgnoreCase)).ToList();

                // Update Users collection on the main thread
                MainThread.BeginInvokeOnMainThread(() =>
                {
                    Users = new ObservableCollection<UserModel>(companyUsers);
                });
            }
            catch (Exception ex)
            {
                ErrorMessage = $"Error loading users: {ex.Message}";
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task CreateNewUser()
        {
            try
            {
                // Check if using Shell-based navigation
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("createuser");
                }
                else
                {
                    // Fallback if not using Shell
                    await Application.Current.MainPage.Navigation.PushAsync(new CreateUserPage());
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Navigation error: {ex.Message}", "OK");
            }
        }


        private async Task EditUser(UserModel user)
        {
            await Shell.Current.DisplayAlert("Edit User", $"Edit user {user.Full_Name} functionality will be implemented here", "OK");
        }

        private async Task DeleteUser(UserModel user)
        {
            try
            {
                // Use Application.Current.MainPage instead of Shell.Current for broader compatibility
                bool confirm = await Application.Current.MainPage.DisplayAlert("Delete User",
                    $"Are you sure you want to delete {user.Full_Name}?", "Yes", "No");

                if (confirm)
                {
                    try
                    {
                        IsLoading = true;
                        bool success = await _apiService.DeleteAsync($"user/{user.Id}");
                        if (success)
                        {
                            // Remove the user from the collection on the main thread
                            MainThread.BeginInvokeOnMainThread(() =>
                            {
                                Users.Remove(user);
                            });

                            // Use Application.Current.MainPage for displaying alert
                            await Application.Current.MainPage.DisplayAlert("Success", "User deleted successfully", "OK");
                        }
                        else
                        {
                            await Application.Current.MainPage.DisplayAlert("Error", "Failed to delete user", "OK");
                        }
                    }
                    catch (Exception ex)
                    {
                        await Application.Current.MainPage.DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
                    }
                    finally
                    {
                        IsLoading = false;
                    }
                }
            }
            catch (Exception ex)
            {
                // Log the error or handle it appropriately
                System.Diagnostics.Debug.WriteLine($"Delete user error: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", "An unexpected error occurred", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
