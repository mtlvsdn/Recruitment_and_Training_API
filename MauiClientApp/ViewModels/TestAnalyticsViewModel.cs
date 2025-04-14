using MauiClientApp.Models;
using MauiClientApp.Services;
using MauiClientApp.Views.Company;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Windows.Input;

namespace MauiClientApp.ViewModels
{
    public class TestAnalyticsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private readonly SessionManager _sessionManager;
        private ObservableCollection<Test> _tests;
        private bool _isLoading;

        public ObservableCollection<Test> Tests
        {
            get => _tests;
            set
            {
                if (_tests != value)
                {
                    _tests = value;
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

        public ICommand LoadTestsCommand { get; }
        public ICommand RefreshCommand { get; }
        public ICommand ViewTestAnalyticsCommand { get; }

        public TestAnalyticsViewModel()
        {
            _apiService = new ApiService();
            _sessionManager = SessionManager.Instance;
            Tests = new ObservableCollection<Test>();

            LoadTestsCommand = new Command(async () => await LoadTestsAsync());
            RefreshCommand = new Command(async () => await LoadTestsAsync());
            ViewTestAnalyticsCommand = new Command<Test>(async (test) => await ViewTestAnalyticsAsync(test));
        }

        private async Task LoadTestsAsync()
        {
            if (IsLoading) return;

            try
            {
                IsLoading = true;
                Tests.Clear();

                // Get the company name from the session
                string companyName = _sessionManager.CompanyName;
                if (string.IsNullOrEmpty(companyName))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Company information not found", "OK");
                    return;
                }

                // Get all tests for the company
                var allTests = await _apiService.GetListAsync<Test>("test");
                var companyTests = allTests.Where(t => t.company_name == companyName).ToList();

                if (companyTests.Any())
                {
                    foreach (var test in companyTests)
                    {
                        Tests.Add(test);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading tests: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load tests: {ex.Message}", "OK");
            }
            finally
            {
                IsLoading = false;
            }
        }
        
        private async Task ViewTestAnalyticsAsync(Test test)
        {
            if (test == null)
            {
                await Application.Current.MainPage.DisplayAlert("Error", "No test selected", "OK");
                return;
            }

            try
            {
                // Navigate to the test analytics details page
                var navigationParameter = new Dictionary<string, object>
                {
                    { "TestId", test.test_id.ToString() }
                };
                
                await Application.Current.MainPage.Navigation.PushAsync(new TestAnalyticsDetailPage(test.test_id));
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error navigating to test analytics: {ex.Message}");
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to open test analytics: {ex.Message}", "OK");
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
} 