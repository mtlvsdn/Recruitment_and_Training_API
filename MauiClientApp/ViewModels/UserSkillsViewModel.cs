using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using MauiClientApp.Models;
using MauiClientApp.Services;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public partial class UserSkillsViewModel : ObservableObject
    {
        private readonly SkillExtractionService _skillExtractionService;
        private readonly SessionManager _sessionManager;

        [ObservableProperty]
        private List<string> hardSkills = new List<string>();

        [ObservableProperty]
        private List<string> softSkills = new List<string>();

        [ObservableProperty]
        private bool isLoading;

        [ObservableProperty]
        private bool hasCv;

        [ObservableProperty]
        private bool hasSkills;

        [ObservableProperty]
        private string statusMessage;

        public ICommand RefreshCommand { get; }
        public ICommand GoBackCommand { get; }

        public UserSkillsViewModel()
        {
            _skillExtractionService = new SkillExtractionService();
            _sessionManager = SessionManager.Instance;
            
            RefreshCommand = new AsyncRelayCommand(LoadSkillsAsync);
            GoBackCommand = new AsyncRelayCommand(GoBackAsync);
            
            // Load skills when the ViewModel is created
            Task.Run(() => LoadSkillsAsync());
        }

        private async Task LoadSkillsAsync()
        {
            try
            {
                IsLoading = true;
                StatusMessage = "Loading skills...";
                
                // Extract skills from the user's CV
                var userSkills = await _skillExtractionService.ExtractSkillsFromCv(_sessionManager.UserId);
                
                // Update observable properties
                HardSkills = userSkills.HardSkills ?? new List<string>();
                SoftSkills = userSkills.SoftSkills ?? new List<string>();
                
                HasCv = HardSkills.Count > 0 || SoftSkills.Count > 0;
                HasSkills = HardSkills.Count > 0 || SoftSkills.Count > 0;
                
                StatusMessage = HasSkills ? 
                    "Skills extracted successfully!" : 
                    "No skills found. Try uploading a more detailed CV.";
            }
            catch (Exception ex)
            {
                StatusMessage = $"Error loading skills: {ex.Message}";
                Console.WriteLine($"Error in LoadSkillsAsync: {ex.Message}");
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