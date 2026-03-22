namespace PrepWise.API.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int InterviewSessionId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? UserAnswer { get; set; }
        public string? Feedback { get; set; }
        public int Score { get; set; }
    }
}
