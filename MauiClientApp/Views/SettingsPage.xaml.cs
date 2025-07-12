using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiClientApp.Services;
using Microsoft.Extensions.Configuration;
using System.Text.Json;

namespace MauiClientApp.Views
{
    public partial class SettingsPage : ContentPage, INotifyPropertyChanged
    {
        private readonly GeminiAIService _geminiService;
        private string _geminiApiKey;

        public string GeminiApiKey
        {
            get => _geminiApiKey;
            set
            {
                if (_geminiApiKey != value)
                {
                    _geminiApiKey = value;
                    OnPropertyChanged();
                }
            }
        }

        public SettingsPage()
        {
            InitializeComponent();
            _geminiService = ServiceHelper.GetService<GeminiAIService>();
            LoadCurrentApiKey();
            BindingContext = this;
        }

        private void LoadCurrentApiKey()
        {
            // Load the API key from preferences
            GeminiApiKey = Preferences.Get("GeminiApiKey", string.Empty);
        }

        private async void OnSaveApiKeyClicked(object sender, EventArgs e)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(GeminiApiKey))
                {
                    await DisplayAlert("Error", "Please enter a valid API key", "OK");
                    return;
                }

                // Save to preferences
                Preferences.Set("GeminiApiKey", GeminiApiKey);
                
                // Update the service with the new API key
                _geminiService.SetApiKey(GeminiApiKey);

                // Show success message
                StatusLabel.Text = "API key saved successfully!";
                StatusLabel.IsVisible = true;
                
                await Task.Delay(2000);
                StatusLabel.IsVisible = false;
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"Failed to save API key: {ex.Message}", "OK");
            }
        }

        private async void OnTestApiConnectionClicked(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(GeminiApiKey))
            {
                await DisplayAlert("Error", "Please save a valid API key first", "OK");
                return;
            }

            try
            {
                TestResultLabel.IsVisible = false;
                
                bool isSuccessful = await _geminiService.TestApiConnection();
                
                if (isSuccessful)
                {
                    TestResultLabel.Text = "✅ Connection successful! The API key is working correctly.";
                    TestResultLabel.TextColor = Colors.Green;
                }
                else
                {
                    TestResultLabel.Text = "❌ Connection failed. Please check your API key.";
                    TestResultLabel.TextColor = Colors.Red;
                }
                
                TestResultLabel.IsVisible = true;
            }
            catch (Exception ex)
            {
                TestResultLabel.Text = $"❌ Connection failed: {ex.Message}";
                TestResultLabel.TextColor = Colors.Red;
                TestResultLabel.IsVisible = true;
            }
        }

        #region INotifyPropertyChanged Implementation
        public event PropertyChangedEventHandler PropertyChanged;
        
        protected void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
        #endregion
    }
} 