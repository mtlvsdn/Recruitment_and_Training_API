namespace MauiClient
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
