using System.ComponentModel.DataAnnotations;

namespace MauiClientApp.Models
{
    public class Test_Results
    {
        [Key]
        public int result_id { get; set; }
        public int Userid { get; set; }
        public int Testtest_id { get; set; }
        public DateTime completion_date { get; set; }
        public int score { get; set; }
        public int total_questions { get; set; }
    }
} 