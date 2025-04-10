using Microsoft.Maui.Controls;
using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.Tests
{
    public partial class CreateQuestionPage : ContentPage
    {
        public CreateQuestionPage()
        {
            InitializeComponent();
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            
            // Make sure view model is updated
            var viewModel = BindingContext as CreateQuestionViewModel;
            if (viewModel != null && viewModel.Test != null)
            {
                viewModel.TotalQuestions = viewModel.Test.NumberOfQuestions;
                // Force UI update
                viewModel.OnPropertyChanged(nameof(viewModel.PageTitle));
            }
        }
    }
} 