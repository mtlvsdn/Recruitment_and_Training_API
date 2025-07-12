using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace MauiClientApp.Models
{
    public class CV_Summarised
    {
        [Key]
        public int cv_sum_id { get; set; }
        
        [Required]
        public int user_id { get; set; }
        
        [Required]
        public int cv_id { get; set; }
        
        [MaxLength(1000)]
        public string soft_skills { get; set; }
        
        [MaxLength(1000)]
        public string hard_skills { get; set; }
    }
} 