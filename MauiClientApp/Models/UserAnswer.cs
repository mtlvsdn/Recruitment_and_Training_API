namespace MauiClientApp.Models
{
    public class UserAnswer
    {
        public int QuestionId { get; set; }
        public string SelectedAnswer { get; set; }
        public bool IsCorrect { get; set; }
        public bool IsAnswered { get; set; }
        
        // Reference to the original question for easy access
        public Question Question { get; set; }
    }
} 