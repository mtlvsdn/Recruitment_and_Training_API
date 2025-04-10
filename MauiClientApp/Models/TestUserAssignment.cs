namespace MauiClientApp.Models
{
    public class TestUserAssignment
    {
        public int Id { get; set; }
        public int TestId { get; set; }
        public int UserId { get; set; }
        public DateTime AssignmentDate { get; set; }
        public DateTime? CompletionDate { get; set; }
        public bool IsCompleted { get; set; }
        public int? Score { get; set; }
    }
} 