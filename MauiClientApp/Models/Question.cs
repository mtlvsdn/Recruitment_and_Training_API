namespace MauiClientApp.Models
{
    public class Question
    {
        public int QuestionId { get; set; }
        public int TestId { get; set; }
        public string QuestionText { get; set; }
        public string PossibleAnswer1 { get; set; }
        public string PossibleAnswer2 { get; set; }
        public string PossibleAnswer3 { get; set; }
        public string PossibleAnswer4 { get; set; }
        public string CorrectAnswer { get; set; }
    }
} 