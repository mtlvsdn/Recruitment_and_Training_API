using MauiClientApp.Models;
using MauiClientApp.Services;
using System;

namespace MauiClientApp.Views.Company
{
    public partial class UserSkillsDetailPage : ContentPage
    {
        private readonly ApiService _apiService;
        
        public UserSkillsDetailPage(Models.User user, string softSkills, string hardSkills)
        {
            InitializeComponent();
            _apiService = new ApiService();
            
            // Set user info
            UserNameLabel.Text = $"{user.Full_Name}";
            EmailLabel.Text = user.Email;
            
            // Set skills
            if (!string.IsNullOrWhiteSpace(softSkills))
            {
                SoftSkillsLabel.Text = softSkills;
            }
            
            if (!string.IsNullOrWhiteSpace(hardSkills))
            {
                HardSkillsLabel.Text = hardSkills;
            }
        }
        
        // Alternative constructor that loads skills from API
        public UserSkillsDetailPage(Models.User user)
        {
            InitializeComponent();
            _apiService = new ApiService();
            
            // Set user info
            UserNameLabel.Text = $"{user.Full_Name}";
            EmailLabel.Text = user.Email;
            
            // Load skills from API
            LoadUserSkills(user.Id);
        }
        
        private async void LoadUserSkills(int userId)
        {
            try
            {
                // Removed loading indicator as requested
                // IsBusy = true;
                
                // Get skills directly using the user ID
                var skills = await _apiService.GetCvSkills(userId);
                
                if (skills != null)
                {
                    SoftSkillsLabel.Text = !string.IsNullOrWhiteSpace(skills.soft_skills) 
                        ? skills.soft_skills 
                        : "No soft skills available";
                        
                    HardSkillsLabel.Text = !string.IsNullOrWhiteSpace(skills.hard_skills) 
                        ? skills.hard_skills 
                        : "No hard skills available";
                }
                else
                {
                    // Check if user has a CV
                    var cv = await _apiService.GetUserCv(userId);
                    if (cv == null)
                    {
                        SoftSkillsLabel.Text = "No CV found for this user";
                        HardSkillsLabel.Text = "No CV found for this user";
                    }
                    else
                    {
                        SoftSkillsLabel.Text = "No skills found for this user";
                        HardSkillsLabel.Text = "No skills found for this user";
                    }
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to load skills: {ex.Message}", "OK");
            }
            finally
            {
                // Removed loading indicator as requested
                // IsBusy = false;
            }
        }
        
        private async void OnBackButtonClicked(object sender, EventArgs e)
        {
            await Navigation.PopAsync();
        }
    }
} 