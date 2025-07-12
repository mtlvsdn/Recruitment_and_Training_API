using System.Text.Json.Serialization;

namespace DeveloperApp
{
    public class CompanyModel
    {
        [JsonPropertyName("Company_Name")]
        public string Company_Name { get; set; }

        [JsonPropertyName("Email")]
        public string Email { get; set; }

        [JsonPropertyName("Password")]
        public string Password { get; set; }

        [JsonPropertyName("Nr_of_accounts")]
        public int LicenseCount { get; set; }

        [JsonPropertyName("SuperUseremail")]
        public string SuperUseremail { get; set; }
    }
}
