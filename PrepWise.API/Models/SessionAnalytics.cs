namespace PrepWise.API.Models
{
    public class SessionAnalytics
    {
        public int Id { get; set; }
        public int SessionId { get; set; }
        public int OverallScore { get; set; }
        public string Strengths { get; set; } = string.Empty;
        public string Weaknesses { get; set; } = string.Empty;
        public string Recommendations { get; set; } = string.Empty;
        public string SkillBreakdown { get; set; } = string.Empty; // JSON
        public string WeakTopics { get; set; } = string.Empty;     // comma-separated
        public DateTime GeneratedAt { get; set; } = DateTime.UtcNow;
    }
}
