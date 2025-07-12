using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using DeveloperApp;
using Microsoft.Maui.Controls;

namespace DeveloperApp.ViewModels
{
    public class AddCompanyViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
#if ANDROID
        private const string ApiBaseUrl = "http://10.0.2.2:7287";
#else
        private const string ApiBaseUrl = "http://localhost:7287";
#endif
        public ObservableCollection<CompanyModel> Companies { get; set; } = new ObservableCollection<CompanyModel>();

        public ICommand AddNewCompany { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public AddCompanyViewModel()
        {
            _httpClient = new HttpClient();
            LoadCompanies();

            AddNewCompany = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new NewCompanyPage()));

            // Pass the selected company to EditCompanyPage
            EditCommand = new Command<CompanyModel>(async (company) =>
            {
                if (company != null)
                {
                    await Application.Current.MainPage.Navigation.PushAsync(new EditCompanyPage(company));
                }
            });

            DeleteCommand = new Command<CompanyModel>(async (company) => await DeleteCompany(company));
        }

        private async void LoadCompanies()
        {
            try
            {
                var companies = await _httpClient.GetFromJsonAsync<List<CompanyModel>>($"{ApiBaseUrl}/company");
                if (companies != null)
                {
                    Companies.Clear();
                    foreach (var company in companies)
                    {
                        Companies.Add(company);
                    }
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading companies: {ex.Message}");
            }
        }

        private async Task DeleteCompany(CompanyModel company)
        {
            bool confirm = await Application.Current.MainPage.DisplayAlert("Confirm",
                $"Are you sure you want to delete {company.Company_Name}?", "Yes", "No");
            if (!confirm) return;

            var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/company/{company.Company_Name}");
            if (response.IsSuccessStatusCode)
            {
                Companies.Remove(company);
                await Application.Current.MainPage.DisplayAlert("Success", "Company deleted successfully", "OK");
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                await Application.Current.MainPage.DisplayAlert("Error", $"Failed to delete company: {errorContent}", "OK");
            }
        }
    }
}