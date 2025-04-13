using MauiClientApp.ViewModels;

namespace MauiClientApp.Views.Tests
{
    [QueryProperty(nameof(Test), "Test")]
    public partial class AssignUsersPage : ContentPage, IQueryAttributable
    {
        private AssignUsersViewModel _viewModel;

        public Models.Test Test
        {
            set
            {
                try
                {
                    Console.WriteLine("AssignUsersPage: Setting Test property");
                    if (value != null)
                    {
                        Console.WriteLine($"AssignUsersPage: Test received - Name: {value.test_name}, ID: {value.test_id}");
                        _viewModel.Initialize(value);
                    }
                    else
                    {
                        Console.WriteLine("AssignUsersPage: Received null Test value");
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"AssignUsersPage: Error setting Test property: {ex.Message}");
                    Console.WriteLine($"Stack trace: {ex.StackTrace}");
                }
            }
        }

        public void ApplyQueryAttributes(IDictionary<string, object> query)
        {
            try
            {
                Console.WriteLine($"AssignUsersPage: ApplyQueryAttributes called with {query.Count} parameters");
                
                if (query.TryGetValue("Test", out var testObject))
                {
                    if (testObject is Models.Test test)
                    {
                        Console.WriteLine($"AssignUsersPage: Found Test in query parameters, ID: {test.test_id}");
                        Test = test;
                    }
                    else
                    {
                        Console.WriteLine($"AssignUsersPage: Test object is of unexpected type: {testObject?.GetType().Name ?? "null"}");
                    }
                }
                else
                {
                    Console.WriteLine("AssignUsersPage: No Test parameter found in query");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignUsersPage: Error in ApplyQueryAttributes: {ex.Message}");
                Console.WriteLine($"AssignUsersPage: Stack trace: {ex.StackTrace}");
            }
        }

        public AssignUsersPage()
        {
            try
            {
                Console.WriteLine("AssignUsersPage: Starting initialization");
                InitializeComponent();
                _viewModel = new AssignUsersViewModel();
                BindingContext = _viewModel;
                Console.WriteLine("AssignUsersPage: Initialization completed");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignUsersPage: Error in constructor: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }

        protected override void OnAppearing()
        {
            try
            {
                base.OnAppearing();
                Console.WriteLine("AssignUsersPage: OnAppearing called");
                _viewModel?.LoadUsersCommand?.Execute(null);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"AssignUsersPage: Error in OnAppearing: {ex.Message}");
                Console.WriteLine($"Stack trace: {ex.StackTrace}");
            }
        }
    }
} 