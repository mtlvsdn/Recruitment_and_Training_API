namespace MauiUserApp.Views;

public partial class CompLoginPage : ContentPage
{
	public CompLoginPage(CompanyLoginPageViewModel viewModel)
	{
		InitializeComponent();
		BindingContext = viewModel;
	}
}