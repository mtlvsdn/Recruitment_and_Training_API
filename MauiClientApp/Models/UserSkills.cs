using System.Collections.Generic;

namespace MauiClientApp.Models
{
    public class UserSkills
    {
        public int UserId { get; set; }
        public List<string> HardSkills { get; set; } = new List<string>();
        public List<string> SoftSkills { get; set; } = new List<string>();
    }
} 