using System;
using System.Net.Http;
using System.Net.Http.Json;
using Microsoft.Maui.Controls;
using Microsoft.Maui.Storage;

namespace DeveloperApp
{
    public partial class NewCompanyPage : ContentPage
    {
        private readonly HttpClient _httpClient;
#if ANDROID
        private const string ApiBaseUrl = "http://10.0.2.2:7287";
#else
        private const string ApiBaseUrl = "http://localhost:7287";
#endif

        public NewCompanyPage()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
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
                //string superUserEmail = Microsoft.Maui.Storage.Preferences.Get("SuperUseremail", "");
                //Console.WriteLine($"SuperUseremail Retrieved: {superUserEmail}");

                //if (string.IsNullOrEmpty(superUserEmail))
                //{
                //    await DisplayAlert("Error", "No SuperUser email found. Please log in again.", "OK");
                //    //await Shell.Current.GoToAsync("//LoginPage");
                //    return;
                //}

                var newCompany = new CompanyModel
                {
                    Company_Name = CompanyNameEntry.Text,
                    Email = EmailEntry.Text,
                    Password = PasswordEntry.Text,
                    LicenseCount = int.TryParse(LicensesEntry.Text, out int count) ? count : 0,
                    SuperUseremail = Preferences.Get("SuperUseremail", "matei@example.com")
                };

                var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/company", newCompany);

                if (response.IsSuccessStatusCode)
                {
                    await DisplayAlert("Success", "Company added successfully", "OK");
                    await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
                }
                else
                {
                    var errorContent = await response.Content.ReadAsStringAsync();
                    await DisplayAlert("Error", $"Failed to add company: {errorContent}", "OK");
                }
            }
            catch (Exception ex)
            {
                await DisplayAlert("Error", $"An error occurred: {ex.Message}", "OK");
            }
        }

        private async void OnCancelClicked(object sender, EventArgs e)
        {
            await Shell.Current.GoToAsync($"//{nameof(HomePage)}");
        }
    }
}