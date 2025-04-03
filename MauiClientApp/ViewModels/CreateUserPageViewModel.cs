using MauiClientApp.Models;
using MauiClientApp.Services;
using MauiClientApp.Views; // For DashboardPage
using MauiClientApp.Views.Company; // For ManageUsers page if needed
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class CreateUserPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        public int Id { get; set; }
        public string FullName { get; set; }
        public string Email { get; set; }
        public string Password { get; set; }
        public string CompanyName { get; set; }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public CreateUserPageViewModel()
        {
            _apiService = new ApiService();
            SaveCommand = new Command(async () => await SaveAsync());
            CancelCommand = new Command(async () => await CancelAsync());
            InitializeDataAsync();
        }

        // Retrieve next available ID and company name from session.
        private async void InitializeDataAsync()
        {
            try
            {
                var users = await _apiService.GetListAsync<UserModel>("user");
                Id = (users != null && users.Any()) ? users.Max(u => u.Id) + 1 : 1;
            }
            catch
            {
                Id = 1;
            }

            // Company name comes from the current session (set during company login)
            CompanyName = SessionManager.Instance.CompanyName;

            OnPropertyChanged(nameof(Id));
            OnPropertyChanged(nameof(CompanyName));
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            // Create a new user object based on the input fields.
            var newUser = new UserModel
            {
                Id = this.Id,
                Full_Name = this.FullName,
                Email = this.Email,
                Password = this.Password,
                Company_Name = this.CompanyName
            };

            try
            {
                // Post the new user to the API
                await _apiService.PostAsync<UserModel>("user", newUser);

                // Use Shell navigation if available, otherwise fallback to Navigation.PushAsync
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//dashboard");
                }
                else if (Application.Current?.MainPage != null)
                {
                    // Fallback navigation: replace DashboardPage with your actual dashboard page reference
                    await Application.Current.MainPage.Navigation.PushAsync(new DashboardPage());
                }
            }
            catch (System.Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error saving user: {ex.Message}", "OK");
            }
        }

        private async System.Threading.Tasks.Task CancelAsync()
        {
            // Use Shell navigation if available, otherwise fallback to Navigation.PushAsync
            try
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//manageusers");
                }
                else if (Application.Current?.MainPage != null)
                {
                    // Fallback navigation: replace ManageUsers with your actual page reference
                    await Application.Current.MainPage.Navigation.PushAsync(new ManageUsers());
                }
            }
            catch (System.Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error during cancel navigation: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
             PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
