using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using MauiClientApp.Models;
using MauiClientApp.Services;

namespace MauiClientApp.ViewModels
{
    public class TestsViewModel : INotifyPropertyChanged
    {
        private readonly ApiService _apiService;
        private ObservableCollection<dynamic> _tests;
        private bool _isLoading;
        private bool _hasTests;
        private bool _showNoTestsMessage;

        public ObservableCollection<dynamic> Tests
        {
            get => _tests;
            set
            {
                _tests = value;
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

        public bool HasTests
        {
            get => _hasTests;
            set
            {
                _hasTests = value;
                OnPropertyChanged();
            }
        }

        public bool ShowNoTestsMessage
        {
            get => _showNoTestsMessage;
            set
            {
                _showNoTestsMessage = value;
                OnPropertyChanged();
            }
        }

        public TestsViewModel()
        {
            _apiService = new ApiService();
            Tests = new ObservableCollection<dynamic>();
        }

        public async Task LoadTestsAsync()
        {
            try
            {
                IsLoading = true;
                ShowNoTestsMessage = false;
                HasTests = false;

                // Get company name from session
                var companyName = SessionManager.Instance.CompanyName;
                if (string.IsNullOrEmpty(companyName))
                {
                    await Application.Current.MainPage.DisplayAlert("Error", "Company name not found in session", "OK");
                    return;
                }

                // Clear existing tests
                Tests.Clear();

                // Get all tests
                var allTests = await _apiService.GetListAsync<dynamic>("test");

                if (allTests != null)
                {
                    // Filter tests by company name
                    var companyTests = allTests.Where(t => 
                        ((IDictionary<string, object>)t).ContainsKey("company_name") && 
                        t.company_name?.ToString() == companyName).ToList();

                    if (companyTests.Any())
                    {
                        foreach (var test in companyTests)
                        {
                            Tests.Add(test);
                        }
                        HasTests = true;
                        ShowNoTestsMessage = false;
                    }
                    else
                    {
                        HasTests = false;
                        ShowNoTestsMessage = true;
                    }
                }
                else
                {
                    HasTests = false;
                    ShowNoTestsMessage = true;
                }
            }
            catch (Exception ex)
            {
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to load tests: {ex.Message}", "OK");
                HasTests = false;
                ShowNoTestsMessage = true;
            }
            finally
            {
                IsLoading = false;
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged([CallerMemberName] string propertyName = null)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }

        #endregion
    }
} 