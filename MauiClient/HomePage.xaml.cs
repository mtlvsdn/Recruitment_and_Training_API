using System.Net.Http;
using System.Threading.Tasks;
using Microsoft.Maui.Controls;
using Newtonsoft.Json;

namespace MauiClient
{
    public partial class HomePage : ContentPage
    {
        private readonly HttpClient _httpClient;

        public HomePage()
        {
            InitializeComponent();
            _httpClient = new HttpClient();
            LoadDataAsync();
        }

        private async Task LoadDataAsync()
        {
            try
            {
                var companyCountResponse = await _httpClient.GetStringAsync("http://10.0.2.2:7287/company/count");
                var companyCount = JsonConvert.DeserializeObject<int>(companyCountResponse);
                CompaniesCountLabel.Text = companyCount.ToString();

                var activeLicensesResponse = await _httpClient.GetStringAsync("http://10.0.2.2:7287/user/count");
                var activeLicenses = JsonConvert.DeserializeObject<int>(activeLicensesResponse);
                ActiveLicensesLabel.Text = activeLicenses.ToString();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error loading data: {ex.Message}");
            }
        }
    }
}
