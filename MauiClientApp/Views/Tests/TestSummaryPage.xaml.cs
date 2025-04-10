using MauiClientApp.ViewModels;
using Microsoft.Maui.Controls;

namespace MauiClientApp.Views.Tests
{
    public partial class TestSummaryPage : ContentPage
    {
        private TestSummaryViewModel _viewModel;

        public TestSummaryPage()
        {
            try
            {
                InitializeComponent();
                _viewModel = BindingContext as TestSummaryViewModel;
                
                // Safety check - if somehow BindingContext is null, create a new one
                if (_viewModel == null)
                {
                    Console.WriteLine("WARNING: TestSummaryPage BindingContext was null, creating a new one");
                    _viewModel = new TestSummaryViewModel();
                    BindingContext = _viewModel;
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TestSummaryPage constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        protected override async void OnAppearing()
        {
            try
            {
                base.OnAppearing();

                if (_viewModel != null)
                {
                    // Make sure Users collection exists
                    if (_viewModel.Users == null)
                    {
                        _viewModel.Users = new System.Collections.ObjectModel.ObservableCollection<UserSelectionItem>();
                    }
                    
                    // Make sure Test object exists
                    if (_viewModel.Test == null)
                    {
                        Console.WriteLine("Creating default Test in OnAppearing");
                        _viewModel.Test = new MauiClientApp.Models.Test
                        {
                            TestName = "New Test",
                            NumberOfQuestions = 0,
                            TimeLimit = 60,
                            Questions = new System.Collections.ObjectModel.ObservableCollection<MauiClientApp.Models.Question>(),
                            AssignedUsers = new System.Collections.ObjectModel.ObservableCollection<MauiClientApp.Models.User>()
                        };
                    }
                    
                    try
                    {
                        await _viewModel.LoadUsersAsync();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine($"Error loading users: {ex.Message}");
                    }
                }
                else
                {
                    Console.WriteLine("WARNING: ViewModel is null in OnAppearing");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in TestSummaryPage.OnAppearing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
} 