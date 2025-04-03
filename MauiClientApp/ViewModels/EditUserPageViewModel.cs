using MauiClientApp.Models;
using MauiClientApp.Services;
using MauiClientApp.Views;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class EditUserPageViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;

        private int _id;
        private string _fullName;
        private string _email;
        private string _password;
        private string _companyName;

        public int Id
        {
            get => _id;
            set { _id = value; OnPropertyChanged(); }
        }
        public string FullName
        {
            get => _fullName;
            set { _fullName = value; OnPropertyChanged(); }
        }
        public string Email
        {
            get => _email;
            set { _email = value; OnPropertyChanged(); }
        }
        public string Password
        {
            get => _password;
            set { _password = value; OnPropertyChanged(); }
        }
        public string CompanyName
        {
            get => _companyName;
            set { _companyName = value; OnPropertyChanged(); }
        }

        public ICommand SaveCommand { get; }
        public ICommand CancelCommand { get; }

        public EditUserPageViewModel(UserModel user)
        {
            _apiService = new ApiService();

            // Prepopulate the fields with the passed user data.
            Id = user.Id;
            FullName = user.Full_Name;
            Email = user.Email;
            Password = user.Password;
            CompanyName = user.Company_Name;

            SaveCommand = new Command(async () => await SaveAsync());
            CancelCommand = new Command(async () => await CancelAsync());
        }

        private async System.Threading.Tasks.Task SaveAsync()
        {
            // Create an updated user object with the new values.
            var updatedUser = new UserModel
            {
                Id = this.Id,
                Full_Name = this.FullName,
                Email = this.Email,
                Password = this.Password,
                Company_Name = this.CompanyName
            };

            try
            {
                // Update the user entry in the database using a PUT request.
                await _apiService.PutAsync<UserModel>($"user/{updatedUser.Id}", updatedUser);

                // Navigate to the DashboardPage (using Shell if available, otherwise fallback)
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//dashboard");
                }
                else if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new DashboardPage());
                }
            }
            catch (System.Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Error updating user: {ex.Message}", "OK");
            }
        }

        private async System.Threading.Tasks.Task CancelAsync()
        {
            // Navigate back to the ManageUsers page.
            try
            {
                if (Shell.Current != null)
                {
                    await Shell.Current.GoToAsync("//manageusers");
                }
                else if (Application.Current?.MainPage != null)
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new MauiClientApp.Views.Company.ManageUsers());
                }
            }
            catch (System.Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Navigation error: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null) =>
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
