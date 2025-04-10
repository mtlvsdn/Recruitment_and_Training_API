using System.Collections.ObjectModel;

namespace MauiClientApp.Models
{
    public class Test
    {
        public int TestId { get; set; }
        public string TestName { get; set; }
        public int NumberOfQuestions { get; set; }
        public int TimeLimit { get; set; }
        public string CompanyName { get; set; }
        public ObservableCollection<Question> Questions { get; set; } = new ObservableCollection<Question>();
        public ObservableCollection<User> AssignedUsers { get; set; } = new ObservableCollection<User>();
    }
} 