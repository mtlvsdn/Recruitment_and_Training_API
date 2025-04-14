namespace MauiClientApp.Models
{
    public class TestResultsData
    {
        // Related to the test
        public int TestId { get; set; }
        public string TestName { get; set; }
        public int TotalQuestions { get; set; }
        
        // Related to the user
        public int UserId { get; set; }
        public string UserName { get; set; }
        public string UserEmail { get; set; }
        
        // Related to the results
        public int CorrectAnswers { get; set; }
        public bool HasTaken { get; set; }
        
        // Calculated properties
        public double ScorePercentage => HasTaken ? (double)CorrectAnswers / TotalQuestions * 100 : 0;
        public string ScoreDisplay => HasTaken ? $"{CorrectAnswers}/{TotalQuestions}" : "Not Taken";
        public string ScorePercentageDisplay => HasTaken ? $"{ScorePercentage:0.#}%" : "Not Taken";
    }
} 