namespace PrepWise.API.Models
{
    public class InterviewSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string InterviewType { get; set; } = "Technical";
        public string Level { get; set; } = "Intermediate";
        public string TargetRole { get; set; } = "";
        public string InterviewGoal { get; set; } = "";
        public string? AdditionalContext { get; set; }
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public List<Question> Questions { get; set; } = new();
    }
}
