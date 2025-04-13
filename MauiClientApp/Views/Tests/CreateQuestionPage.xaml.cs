using Microsoft.Maui.Controls;
using MauiClientApp.ViewModels;
using MauiClientApp.Models;
using System.Diagnostics;
using System.Linq;

namespace MauiClientApp.Views.Tests
{
    public partial class CreateQuestionPage : ContentPage, IQueryAttributable
    {
        private CreateQuestionViewModel _viewModel;
        private bool _isEditMode;

        public CreateQuestionPage()
        {
            InitializeComponent();
            _viewModel = new CreateQuestionViewModel();
            BindingContext = _viewModel;
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Console.WriteLine($"CreateQuestionPage: ApplyQueryAttributes called with {query.Count} parameters");
            
            // Check if we're in edit mode
            if (query.TryGetValue("IsEditMode", out var isEditModeObj) && isEditModeObj is bool editMode)
            {
                _isEditMode = editMode;
                Console.WriteLine($"CreateQuestionPage: Edit mode is {_isEditMode}");
            }
            
            // Pass the parameters to the view model
            if (_viewModel != null)
            {
                _viewModel.ApplyQueryAttributes(query);
                
                // If in edit mode, load the existing question
                if (_isEditMode && _viewModel.Test != null && _viewModel.QuestionNumber > 0)
                {
                    LoadExistingQuestion();
                }
            }
            else
            {
                Console.WriteLine("CreateQuestionPage: ViewModel is null in ApplyQueryAttributes");
            }
        }

        private async void LoadExistingQuestion()
        {
            try
            {
                Console.WriteLine($"CreateQuestionPage: Loading existing question {_viewModel.QuestionNumber} for test {_viewModel.Test.test_id}");
                
                // Get the question from the API based on test ID and question number
                var apiService = new MauiClientApp.Services.ApiService();
                var questions = await apiService.GetListAsync<Question>($"questions/test/{_viewModel.Test.test_id}");
                
                if (questions != null && questions.Any())
                {
                    // Order the questions by ID to ensure consistent ordering
                    var orderedQuestions = questions.OrderBy(q => q.question_id).ToList();
                    
                    // Display how many questions are found for debugging
                    Console.WriteLine($"CreateQuestionPage: {orderedQuestions.Count} questions found for test {_viewModel.Test.test_id}");
                    Console.WriteLine($"CreateQuestionPage: Question IDs: {string.Join(", ", orderedQuestions.Select(q => q.question_id))}");
                    
                    // Check if we have enough questions for this position
                    if (_viewModel.QuestionNumber <= orderedQuestions.Count)
                    {
                        // Get the appropriate question (adjust index as needed based on your data structure)
                        // Since arrays/lists are 0-based but our QuestionNumber is 1-based, subtract 1
                        var question = orderedQuestions[_viewModel.QuestionNumber - 1];
                        
                        // Update the view model with the question data
                        _viewModel.QuestionText = question.question_text;
                        _viewModel.AnswerA = question.possible_answer_1;
                        _viewModel.AnswerB = question.possible_answer_2;
                        _viewModel.AnswerC = question.possible_answer_3;
                        _viewModel.AnswerD = question.possible_answer_4;
                        _viewModel.CorrectAnswer = question.correct_answer;
                        _viewModel.QuestionId = question.question_id; // Store the question ID for updates
                        
                        // Mark as loaded so we don't try to load it again
                        _viewModel.IsQuestionLoaded = true;
                        
                        Console.WriteLine($"CreateQuestionPage: Loaded question ID: {question.question_id}, text: {question.question_text}");
                    }
                    else
                    {
                        Console.WriteLine($"CreateQuestionPage: Question number {_viewModel.QuestionNumber} exceeds count of {orderedQuestions.Count}");
                        // Display an error dialog to the user
                        await Application.Current.MainPage.DisplayAlert("Error", 
                            $"No question found for position {_viewModel.QuestionNumber}. You can create one now.", "OK");
                    }
                }
                else
                {
                    Console.WriteLine($"CreateQuestionPage: No questions found for test {_viewModel.Test.test_id}");
                    // Display an informational dialog to the user
                    await Application.Current.MainPage.DisplayAlert("Information", 
                        "No questions found for this test. You can create them now.", "OK");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"CreateQuestionPage: Error loading question: {ex.Message}");
                Console.WriteLine($"CreateQuestionPage: Stack trace: {ex.StackTrace}");
                await Application.Current.MainPage.DisplayAlert("Error", 
                    $"Failed to load question: {ex.Message}", "OK");
            }
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Make sure view model is updated
            if (_viewModel != null && _viewModel.Test != null)
            {
                _viewModel.TotalQuestions = _viewModel.Test.NumberOfQuestions;
                // Force UI update
                _viewModel.OnPropertyChanged(nameof(_viewModel.PageTitle));
                
                // If we're in edit mode and this is the first time appearing, load existing question
                if (_isEditMode && !_viewModel.IsQuestionLoaded)
                {
                    LoadExistingQuestion();
                }
            }
        }
    }
} 