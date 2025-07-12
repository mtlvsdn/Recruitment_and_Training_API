using DeveloperApp.ViewModels;

namespace DeveloperApp
{
    public partial class AddCompanyPage : ContentPage
    {
        public AddCompanyPage()
        {
            InitializeComponent();
            BindingContext = new AddCompanyViewModel();
        }
    }
}
