using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Net.Http.Json;
using System.Threading.Tasks;
using System.Windows.Input;
using MauiClient.ViewModels;
using Microsoft.Maui.Controls;

namespace MauiClient
{
    public class AddCompanyViewModel : BaseViewModel
    {
        private readonly HttpClient _httpClient;
#if __ANDROID__
        private const string ApiBaseUrl = "http://10.0.2.2:7287";
#else
        private const string ApiBaseUrl = "http://localhost:7287";
#endif

        public ObservableCollection<CompanyModel> Companies { get; set; } = new ObservableCollection<CompanyModel>();

        // These commands are defined in the view model.
        public ICommand AddNewCompany { get; }
        public ICommand EditCommand { get; }
        public ICommand DeleteCommand { get; }

        public AddCompanyViewModel()
        {
            _httpClient = new HttpClient();
            LoadCompanies();

            AddNewCompany = new Command(async () => await Application.Current.MainPage.Navigation.PushAsync(new NewCompanyPage()));
            EditCommand = new Command<CompanyModel>(async (company) => await EditCompany(company));
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

        private async Task AddCompany()
        {
            // Use the API property names: Company_Name and LicenseCount.
            var newCompany = new CompanyModel { Company_Name = "New Company", LicenseCount = 1 };
            var response = await _httpClient.PostAsJsonAsync($"{ApiBaseUrl}/company", newCompany);

            if (response.IsSuccessStatusCode)
            {
                Companies.Add(newCompany);
            }
        }

        private async Task EditCompany(CompanyModel company)
        {
            var response = await _httpClient.PutAsJsonAsync($"{ApiBaseUrl}/company/{company.Company_Name}", company);
            if (response.IsSuccessStatusCode)
            {
                LoadCompanies();
            }
        }

        private async Task DeleteCompany(CompanyModel company)
        {
            var response = await _httpClient.DeleteAsync($"{ApiBaseUrl}/company/{company.Company_Name}");
            if (response.IsSuccessStatusCode)
            {
                Companies.Remove(company);
            }
        }
    }
}
