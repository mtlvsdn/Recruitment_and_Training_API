using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace MauiClientApp.Services
{
    public class ApiService
    {
        private readonly HttpClient _httpClient;
#if ANDROID
        private const string _baseUrl = "http://10.0.2.2:7287";
#else
        private const string _baseUrl = "http://localhost:7287";
#endif

        public ApiService()
        {
            _httpClient = new HttpClient();
        }

        public async Task<T> GetAsync<T>(string endpoint)
        {
            var response = await _httpClient.GetAsync($"{_baseUrl}/{endpoint}");
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<T> PostAsync<T>(string endpoint, object data)
        {
            var json = JsonSerializer.Serialize(data);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync($"{_baseUrl}/{endpoint}", content);
            response.EnsureSuccessStatusCode();
            return await response.Content.ReadFromJsonAsync<T>();
        }

        public async Task<bool> AuthenticateCompanyAsync(string email, string password)
        {
            try
            {
                var loginData = new { Email = email, Password = password };
                var response = await PostAsync<AuthResponse>("authenticate-company", loginData);
                return !string.IsNullOrEmpty(response.Token);
            }
            catch (Exception)
            {
                return false;
            }
        }
    }

    public class AuthResponse
    {
        public string Token { get; set; }
        public string Email { get; set; }
    }
}