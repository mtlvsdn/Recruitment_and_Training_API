using MauiClientApp.Models;
using MauiClientApp.Services;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class ViewCvsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly SessionManager _sessionManager;
        private ObservableCollection<User> _users;
        private bool _isLoading;

        public ObservableCollection<User> Users
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

        public ICommand LoadUsersCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ViewCvCommand { get; }
        public ICommand ViewSkillsCommand { get; }

        public ViewCvsViewModel()
        {
            _apiService = new ApiService();
            _sessionManager = SessionManager.Instance;
            Users = new ObservableCollection<User>();

            LoadUsersCommand = new Command(async () => await LoadUsersAsync());
            RefreshCommand = new Command(async () => await LoadUsersAsync());
            ViewCvCommand = new Command<User>(async (user) => await ViewCvAsync(user));
            ViewSkillsCommand = new Command<User>(async (user) => await ViewSkillsAsync(user));
        }

        private async Task LoadUsersAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Users.Clear();

                // Get the company name from the session
                string companyName = _sessionManager.CompanyName;
                if (string.IsNullOrEmpty(companyName))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Company information not found", "OK");
                    return;
                }

                // Get all users for the company
                var allUsers = await _apiService.GetListAsync<User>("user");
                var companyUsers = allUsers.Where(u => u.Company_Name == companyName).ToList();

                if (companyUsers.Any())
                {
                    foreach (var user in companyUsers)
                    {
                        Users.Add(user);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading users: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load users: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ViewCvAsync(User user)
        {
            if (user == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No user selected", "OK");
                return;
            }

            try
            {
                // Show a loading indicator
                IsLoading = true;
                
                // Get the CV for the user
                var cv = await _apiService.GetAsync<CvPdf>($"cv-pdf/{user.Id}");
                
                if (cv == null || cv.file_data == null || cv.file_data.Length == 0)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "No Resume Available", 
                        $"{user.Full_Name} does not have a resume uploaded in the system. Please have them upload their resume to view it here.", 
                        "OK");
                    return;
                }
                
                // Create a temporary file in the cache directory
                string cacheDir = FileSystem.CacheDirectory;
                string tempFilePath = Path.Combine(cacheDir, cv.file_name);
                
                // Write the file data to the temporary file
                await File.WriteAllBytesAsync(tempFilePath, cv.file_data);
                
                // Open the file with the default PDF viewer
                await Launcher.OpenAsync(new OpenFileRequest
                {
                    File = new ReadOnlyFile(tempFilePath)
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error viewing CV: {ex.Message}");
                // For other errors, still provide a clear message
                await Application.Current.MainPage.DisplayAlert(
                    "Unable to Open Resume", 
                    $"There was a problem accessing {user.Full_Name}'s resume. The resume might be damaged or unavailable at this time.", 
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task ViewSkillsAsync(User user)
        {
            if (user == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No user selected", "OK");
                return;
            }

            try
            {
                // Show loading indicator
                IsLoading = true;
                
                // Try to get the CV summary data for this user
                var cvSummary = await _apiService.GetListAsync<CvSummarised>("cv-summarised");
                var userCvSummary = cvSummary.FirstOrDefault(cv => cv.user_id == user.Id);
                
                if (userCvSummary == null)
                {
                    await Application.Current.MainPage.DisplayAlert(
                        "No Skills Information", 
                        $"{user.Full_Name} does not have any skills information in the system. Skills will be available after they upload a resume and the system extracts skill information from it.", 
                        "OK");
                    return;
                }
                
                // Format skills for better readability
                string hardSkills = !string.IsNullOrEmpty(userCvSummary.hard_skills) 
                    ? FormatSkillsList(userCvSummary.hard_skills) 
                    : "No technical skills identified in resume";
                    
                string softSkills = !string.IsNullOrEmpty(userCvSummary.soft_skills)
                    ? FormatSkillsList(userCvSummary.soft_skills)
                    : "No interpersonal skills identified in resume";
                
                // Display skills in a popup with improved formatting
                string message = $"Technical Skills:\n{hardSkills}\n\nInterpersonal Skills:\n{softSkills}";
                
                await Application.Current.MainPage.DisplayAlert(
                    $"{user.Full_Name}'s Skills Profile", 
                    message, 
                    "Close");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error viewing skills: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert(
                    "Skills Information Unavailable", 
                    $"The system could not retrieve skill information for {user.Full_Name}. This could be due to a system error or missing data.", 
                    "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }

        // Helper method to format skills as a bullet list
        private string FormatSkillsList(string skills)
        {
            if (string.IsNullOrEmpty(skills))
                return "None";
            
            // Split by commas or semicolons and trim whitespace
            var skillsList = skills.Split(new[] { ',', ';' }, StringSplitOptions.RemoveEmptyEntries)
                .Select(s => s.Trim())
                .Where(s => !string.IsNullOrEmpty(s));
            
            // Format as bullet points
            return string.Join("\n", skillsList.Select(s => $"• {s}"));
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 