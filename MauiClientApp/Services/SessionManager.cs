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

        public void ClearSession()
        {
            Token = null;
            Email = null;
            CompanyName = null;
            UserFullName = null;
            UserId = 0;
        }
    }
}
