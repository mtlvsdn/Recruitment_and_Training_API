using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;
using System.Collections.ObjectModel;

namespace MauiClientApp.ViewModels
{
    public partial class ViewSkillsViewModel : ObservableObject
    {
        private readonly ApiService _apiService;

        [ObservableProperty]
        private ObservableCollection<string> _hardSkills = new();

        [ObservableProperty]
        private ObservableCollection<string> _softSkills = new();

        [ObservableProperty]
        private bool _isLoading;

        [ObservableProperty]
        private bool _hasSkills;

        [ObservableProperty]
        private string _statusMessage = string.Empty;

        public IRelayCommand GoBackCommand { get; }
        public IRelayCommand InitializeCommand { get; }

        public ViewSkillsViewModel()
        {
            _apiService = new ApiService();
            GoBackCommand = new AsyncRelayCommand(GoBackAsync);
            InitializeCommand = new AsyncRelayCommand(LoadSkillsAsync);
            
            // Load skills when the page is first shown
            Task.Run(() => LoadSkillsAsync());
        }

        private async Task LoadSkillsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading skills...";
                
                // Clear existing skills
                HardSkills.Clear();
                SoftSkills.Clear();
                
                // Get the current user's ID
                int userId = SessionManager.Instance.UserId;
                
                // Get the CV summary for this user
                var cvSummary = await _apiService.GetAsync<CV_Summarised>($"cv-summarised/by-user/{userId}");
                
                if (cvSummary != null)
                {
                    // Process hard skills
                    if (!string.IsNullOrEmpty(cvSummary.hard_skills))
                    {
                        var hardSkillsList = cvSummary.hard_skills.Split(',')
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                        
                        foreach (var skill in hardSkillsList)
                        {
                            HardSkills.Add(skill);
                        }
                    }
                    
                    // Process soft skills
                    if (!string.IsNullOrEmpty(cvSummary.soft_skills))
                    {
                        var softSkillsList = cvSummary.soft_skills.Split(',')
                            .Select(s => s.Trim())
                            .Where(s => !string.IsNullOrEmpty(s))
                            .ToList();
                        
                        foreach (var skill in softSkillsList)
                        {
                            SoftSkills.Add(skill);
                        }
                    }
                    
                    HasSkills = HardSkills.Count > 0 || SoftSkills.Count > 0;
                }
                else
                {
                    StatusMessage = "No skills found. Please upload your CV.";
                    HasSkills = false;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading skills: {ex.Message}");
                StatusMessage = $"Error loading skills: {ex.Message}";
                HasSkills = false;
            }
            finally
            {
                IsLoading = false;
            }
        }

        private async Task GoBackAsync()
        {
            await Application.Current.MainPage.Navigation.PopAsync();
        }
    }
} 