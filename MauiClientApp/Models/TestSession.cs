using System.Collections.ObjectModel;
using System.Collections.Generic;

namespace MauiClientApp.Models
{
    public class TestSession
    {
        public Test Test { get; set; }
        public List<Question> Questions { get; set; }
        public ObservableCollection<UserAnswer> UserAnswers { get; set; }
        public DateTime StartTime { get; set; }
        public DateTime EndTime { get; set; }
        public int CurrentQuestionIndex { get; set; }
        public bool IsCompleted { get; set; }
        public int CorrectAnswers { get; set; }
        public int TotalQuestions => Questions?.Count ?? 0;
        public TimeSpan RemainingTime => Test != null 
            ? TimeSpan.FromMinutes(Test.time_limit) - (DateTime.Now - StartTime) 
            : TimeSpan.Zero;
        
        public TestSession()
        {
            Test = new Test();
            Questions = new List<Question>();
            UserAnswers = new ObservableCollection<UserAnswer>();
            StartTime = DateTime.Now;
            CurrentQuestionIndex = 0;
            IsCompleted = false;
            CorrectAnswers = 0;
        }
    }
} 