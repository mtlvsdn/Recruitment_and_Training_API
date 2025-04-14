namespace MauiClientApp.Services
{
    public class SessionManager
    {
        private static SessionManager _instance;
        private static readonly object _lock = new object();

        public static SessionManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    lock (_lock)
                    {
                        if (_instance == null)
                            _instance = new SessionManager();
                    }
                }
                return _instance;
            }
        }

        public string Token { get; private set; }
        public string Email { get; private set; }
        public string CompanyName { get; private set; }
        public string UserFullName { get; private set; }
        public int UserId { get; private set; }
        public bool IsLoggedIn => !string.IsNullOrEmpty(Token);
        public bool IsCompanyLogin => !string.IsNullOrEmpty(CompanyName) && string.IsNullOrEmpty(UserFullName);
        public bool IsUserLogin => !string.IsNullOrEmpty(UserFullName);

        public void SetSession(string token, string email, string companyName = null)
        {
            Token = token;
            Email = email;
            CompanyName = companyName;
        }

        public void SetUserDetails(string fullName, string companyName, int userId)
        {
            UserFullName = fullName;
            CompanyName = companyName;
            UserId = userId;
        }

        public async Task SaveSessionAsync()
        {
            if (IsUserLogin)
            {
                await SecureStorageService.SaveUserLoginAsync(Email, Token, UserFullName, CompanyName, UserId);
            }
            else if (IsCompanyLogin)
            {
                await SecureStorageService.SaveCompanyLoginAsync(Email, Token, CompanyName);
            }
        }

        public async Task<bool> LoadSessionAsync()
        {
            if (!await SecureStorageService.HasSavedLoginAsync())
                return false;

            var loginType = await SecureStorageService.GetLoginTypeAsync();
            
            if (loginType == "user")
            {
                var userData = await SecureStorageService.GetUserLoginDataAsync();
                if (userData == null)
                    return false;
                
                Token = userData["Token"];
                Email = userData["Email"];
                UserFullName = userData["FullName"];
                CompanyName = userData["CompanyName"];
                UserId = int.Parse(userData["UserId"]);
                return true;
            }
            else if (loginType == "company")
            {
                var companyData = await SecureStorageService.GetCompanyLoginDataAsync();
                if (companyData == null)
                    return false;
                
                Token = companyData["Token"];
                Email = companyData["Email"];
                CompanyName = companyData["CompanyName"];
                return true;
            }
            
            return false;
        }

        public void ClearSession()
        {
            Token = null;
            Email = null;
            CompanyName = null;
            UserFullName = null;
            UserId = 0;
            
            // Also clear the secure storage
            _ = SecureStorageService.ClearLoginDataAsync();
        }
    }
}
