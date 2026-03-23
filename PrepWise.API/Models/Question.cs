namespace PrepWise.API.Models
{
    public class Question
    {
        public int Id { get; set; }
        public int InterviewSessionId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "Conceptual"; // Conceptual | Scenario | ProblemSolving | Behavioral
        public string Difficulty { get; set; } = "Medium";       // Easy | Medium | Hard
        public string Source { get; set; } = "AI";               // AI | CV
        public string? UserAnswer { get; set; }
        public string? Feedback { get; set; }
        public string? SuggestedAnswer { get; set; }
        public string? MissingPoints { get; set; }
        public int Score { get; set; }
        public bool IsFollowUp { get; set; } = false;
    }
}
