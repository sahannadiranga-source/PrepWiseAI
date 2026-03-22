namespace PrepWise.Client.Models
{
    public class InterviewSession
    {
        public int Id { get; set; }
        public string InterviewType { get; set; } = string.Empty;
        public DateTime DateCreated { get; set; }
        public List<Question> Questions { get; set; } = new();
    }

    public class Question
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? UserAnswer { get; set; }
        public string? Feedback { get; set; }
        public int Score { get; set; }
    }
}