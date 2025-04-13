namespace MauiClientApp.Models
{
    public class Question
    {
        public int question_id { get; set; }
        public int test_id { get; set; }
        public string question_text { get; set; }
        public string possible_answer_1 { get; set; }
        public string possible_answer_2 { get; set; }
        public string possible_answer_3 { get; set; }
        public string possible_answer_4 { get; set; }
        public string correct_answer { get; set; }
    }
} 