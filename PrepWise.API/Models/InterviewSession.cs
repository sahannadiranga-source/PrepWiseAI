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
        public string ExperienceLevel { get; set; } = "Junior"; // Intern, Junior, Mid-level, Senior
        public string Mode { get; set; } = "Text"; // Text | Timed
        public string? CvExtractedSkills { get; set; } // JSON array of skill strings
        public string? CvProfile { get; set; }          // Full CV profile for project-specific questions
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public List<Question> Questions { get; set; } = new();
        public SessionAnalytics? Analytics { get; set; }
    }
}
