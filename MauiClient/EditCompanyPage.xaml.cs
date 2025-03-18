using System.Net.Http.Json;

namespace MauiClient;

public partial class EditCompanyPage : ContentPage
{
    private readonly HttpClient _httpClient;
    private CompanyModel _companyToEdit;
    private bool _isNewCompany = true;

#if ANDROID
    private const string ApiBaseUrl = "http://10.0.2.2:7287";
#else
    private const string ApiBaseUrl = "http://localhost:7287";
#endif

    public EditCompanyPage()
    {
        InitializeComponent();
        _httpClient = new HttpClient();
    }

    // Constructor for editing existing company
    public EditCompanyPage(CompanyModel company)
    {
        InitializeComponent();
        _httpClient = new HttpClient();
        _companyToEdit = company;
        _isNewCompany = false;

        // Populate the form with existing company data
        CompanyNameEntry.Text = company.Company_Name;
        EmailEntry.Text = company.Email;
        PasswordEntry.Text = company.Password;
        LicensesEntry.Text = company.LicenseCount.ToString();

        // Disable and style the company name field
        CompanyNameEntry.IsEnabled = false;
        CompanyNameEntry.BackgroundColor = Colors.LightGray;
        CompanyNameEntry.TextColor = Colors.Gray;

        // Update UI to show we're editing
        Title = "Edit Company";
        AddButton.Text = "Update";
    }

    private async void OnAddClicked(object sender, EventArgs e)
    {
        if (string.IsNullOrWhiteSpace(CompanyNameEntry.Text) ||
            string.IsNullOrWhiteSpace(EmailEntry.Text) ||
            string.IsNullOrWhiteSpace(PasswordEntry.Text) ||
            string.IsNullOrWhiteSpace(LicensesEntry.Text))
        {
            await DisplayAlert("Error", "All fields are required", "OK");
            return;
        }

        try
        {
            var company = new CompanyModel
            {
                Company_Name = CompanyNameEntry.Text,
                Email = EmailEntry.Text,
                Password = PasswordEntry.Text,
                LicenseCount = int.TryParse(LicensesEntry.Text, out int count) ? count : 0,
                SuperUseremail = Preferences.Get("SuperUseremail", "matei@example.com")
            };

            HttpResponseMessage response;

            Console.WriteLine($"Sending request to {(_isNewCompany ? "create" : "update")} company: {System.Text.Json.JsonSerializer.Serialize(company)}");

            if (_isNewCompany)
            {
                // Use POST for new companies
                response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/company", company);
            }
            else
            {
                // Use PUT for existing companies
                // Use the original company name
                string companyName = _companyToEdit.Company_Name;
                response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}/company/{companyName}", company);
            }

            if (response.IsSuccessStatusCode)
            {
                string message = _isNewCompany ? "Company added successfully" : "Company updated successfully";
                await DisplayAlert("Success", message, "OK");
                await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                Console.WriteLine($"Error response: {response.StatusCode} - {errorContent}");
                await DisplayAlert("Error", $"Failed to {(_isNewCompany ? "add" : "update")} company: {errorContent}", "OK");
            }
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Exception: {ex}");
            await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
        }
    }

    private async void OnCancelClicked(object sender, EventArgs e)
    {
        await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
    }
}