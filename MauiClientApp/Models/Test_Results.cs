using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MauiClientApp.Models
{
    public class Test_Results
    {
        [Key]
        public int result_id { get; set; }

        [Column("user_id")]
        public int Userid { get; set; }

        [Column("test_id")]
        public int Testtest_id { get; set; }

        public DateTime completion_date { get; set; }
        public int score { get; set; }
        public int total_questions { get; set; }
    }
} 