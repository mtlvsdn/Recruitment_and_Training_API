using System.Text.Json;

namespace MauiClientApp.Services
{
    public static class SecureStorageService
    {
        private const string USER_LOGIN_KEY = "user_login_data";
        private const string COMPANY_LOGIN_KEY = "company_login_data";
        private const string LOGIN_TYPE_KEY = "login_type";
        
        public static async Task SaveUserLoginAsync(string email, string token, string fullName, string companyName, int userId)
        {
            var userData = new Dictionary<string, string>
            {
                { "Email", email },
                { "Token", token },
                { "FullName", fullName },
                { "CompanyName", companyName },
                { "UserId", userId.ToString() }
            };
            
            string userJson = JsonSerializer.Serialize(userData);
            await SecureStorage.SetAsync(USER_LOGIN_KEY, userJson);
            await SecureStorage.SetAsync(LOGIN_TYPE_KEY, "user");
            
            Console.WriteLine("Saved user login data to secure storage");
        }
        
        public static async Task SaveCompanyLoginAsync(string email, string token, string companyName)
        {
            var companyData = new Dictionary<string, string>
            {
                { "Email", email },
                { "Token", token },
                { "CompanyName", companyName }
            };
            
            string companyJson = JsonSerializer.Serialize(companyData);
            await SecureStorage.SetAsync(COMPANY_LOGIN_KEY, companyJson);
            await SecureStorage.SetAsync(LOGIN_TYPE_KEY, "company");
            
            Console.WriteLine("Saved company login data to secure storage");
        }
        
        public static async Task<bool> HasSavedLoginAsync()
        {
            var loginType = await SecureStorage.GetAsync(LOGIN_TYPE_KEY);
            return !string.IsNullOrEmpty(loginType);
        }
        
        public static async Task<string> GetLoginTypeAsync()
        {
            return await SecureStorage.GetAsync(LOGIN_TYPE_KEY);
        }
        
        public static async Task<Dictionary<string, string>> GetUserLoginDataAsync()
        {
            var json = await SecureStorage.GetAsync(USER_LOGIN_KEY);
            if (string.IsNullOrEmpty(json))
                return null;
                
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        
        public static async Task<Dictionary<string, string>> GetCompanyLoginDataAsync()
        {
            var json = await SecureStorage.GetAsync(COMPANY_LOGIN_KEY);
            if (string.IsNullOrEmpty(json))
                return null;
                
            return JsonSerializer.Deserialize<Dictionary<string, string>>(json);
        }
        
        public static async Task ClearLoginDataAsync()
        {
            // Remove all saved credentials
            SecureStorage.Remove(USER_LOGIN_KEY);
            SecureStorage.Remove(COMPANY_LOGIN_KEY);
            SecureStorage.Remove(LOGIN_TYPE_KEY);
            
            Console.WriteLine("Cleared all login data from secure storage");
        }
    }
} 