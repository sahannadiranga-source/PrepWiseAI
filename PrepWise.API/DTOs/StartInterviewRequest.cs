namespace PrepWise.API.DTOs
{
    public class StartInterviewRequest
    {
        public int UserId { get; set; }
        public string InterviewType { get; set; } = "Technical";
        public string Level { get; set; } = "Intermediate"; // Beginner, Intermediate, Pro
        public string TargetRole { get; set; } = "";
        public string InterviewGoal { get; set; } = ""; // e.g. Internship, Junior, Senior
        public string? AdditionalContext { get; set; }
    }
}