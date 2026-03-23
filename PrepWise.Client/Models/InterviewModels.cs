namespace PrepWise.Client.Models
{
    public class InterviewSession
    {
        public int Id { get; set; }
        public string InterviewType { get; set; } = string.Empty;
        public string Level { get; set; } = string.Empty;
        public string TargetRole { get; set; } = string.Empty;
        public string InterviewGoal { get; set; } = string.Empty;
        public string ExperienceLevel { get; set; } = string.Empty;
        public string Mode { get; set; } = "Text";
        public string? CvExtractedSkills { get; set; }
        public DateTime DateCreated { get; set; }
        public List<Question> Questions { get; set; } = new();
        public SessionAnalytics? Analytics { get; set; }
    }

    public class Question
    {
        public int Id { get; set; }
        public string Content { get; set; } = string.Empty;
        public string QuestionType { get; set; } = "Conceptual";
        public string Difficulty { get; set; } = "Medium";
        public string? UserAnswer { get; set; }
        public string? Feedback { get; set; }
        public string? SuggestedAnswer { get; set; }
        public string? MissingPoints { get; set; }
        public int Score { get; set; }
    }

    public class SessionAnalytics
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int OverallScore { get; set; }
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public string SkillBreakdown { get; set; } = string.Empty;
        public string WeakTopics { get; set; } = string.Empty; // comma-separated
        public DateTime GeneratedAt { get; set; }
    }

    public class UserSkill
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string Source { get; set; } = "CV";
    }
}
