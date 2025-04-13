using System.Collections.ObjectModel;

namespace MauiClientApp.Models
{
    public class Test
    {
        public int test_id { get; set; }
        public string test_name { get; set; }
        public int no_of_questions { get; set; }
        public int time_limit { get; set; }
        public string company_name { get; set; }
        
        // Client-side properties for UI binding (keeping these in PascalCase for XAML binding)
        public string TestName 
        { 
            get => test_name;
            set => test_name = value;
        }
        
        public int NumberOfQuestions 
        { 
            get => no_of_questions;
            set => no_of_questions = value;
        }
        
        public int TimeLimit 
        { 
            get => time_limit;
            set => time_limit = value;
        }
        
        public string CompanyName 
        { 
            get => company_name;
            set => company_name = value;
        }
        
        public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();
        public ObservableCollection<User> AssignedUsers { get; set; } = new ObservableCollection<User>();
    }
} 