using MauiClientApp.Models;
using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.Tests
{
    [QueryProperty(nameof(TestId), "TestId")]
    public partial class EditTestPage : ContentPage, IQueryAttributable
    {
        private string _testId;
        private EditTestViewModel _viewModel;

        public string TestId
        {
            get => _testId;
            set
            {
                Console.WriteLine($"EditTestPage: TestId property set to {value}");
                _testId = value;
                LoadTestData();
            }
        }

        private void LoadTestData()
        {
            if (_viewModel != null && !string.IsNullOrEmpty(_testId) && int.TryParse(_testId, out int id))
            {
                Console.WriteLine($"EditTestPage: Setting Test in ViewModel with ID: {id}");
                _viewModel.Test = new Test { test_id = id };
            }
            else
            {
                if (_viewModel == null)
                    Console.WriteLine("EditTestPage: _viewModel is null");
                if (string.IsNullOrEmpty(_testId))
                    Console.WriteLine("EditTestPage: _testId is null or empty");
                else if (!int.TryParse(_testId, out _))
                    Console.WriteLine($"EditTestPage: Could not parse _testId: {_testId}");
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            Console.WriteLine($"EditTestPage: ApplyQueryAttributes called with {query.Count} parameters");
            
            if (query.TryGetValue("TestId", out var testIdObj))
            {
                Console.WriteLine($"EditTestPage: Found TestId in query: {testIdObj}");
                
                // Handle different types that could be passed
                if (testIdObj is string testIdStr)
                {
                    TestId = testIdStr;
                }
                else if (testIdObj is int testIdInt)
                {
                    TestId = testIdInt.ToString();
                }
                else
                {
                    Console.WriteLine($"EditTestPage: TestId is of unexpected type: {testIdObj?.GetType().Name ?? "null"}");
                }
            }
            else
            {
                Console.WriteLine("EditTestPage: TestId not found in query parameters");
            }
        }

        public EditTestPage()
        {
            Console.WriteLine("EditTestPage: Constructor called");
            InitializeComponent();
            _viewModel = new EditTestViewModel();
            BindingContext = _viewModel;
            Console.WriteLine("EditTestPage: ViewModel created and assigned to BindingContext");
        }

        protected override void OnAppearing()
        {
            base.OnAppearing();
            Console.WriteLine("EditTestPage: OnAppearing called");
            LoadTestData();
        }
    }
} 