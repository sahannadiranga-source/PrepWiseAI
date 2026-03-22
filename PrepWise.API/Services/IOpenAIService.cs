namespace PrepWise.API.Services
{
    public interface IOpenAIService
    {
        Task<string> GenerateQuestionAsync(string topic, string level = "Intermediate", string targetRole = "",
            string interviewGoal = "", string? additionalContext = null, List<string>? cvSkills = null,
            string questionType = "Conceptual", string difficulty = "Medium",
            string? cvProfile = null, List<(string Q, string A)>? previousQA = null);

        Task<(string Feedback, int Score, string SuggestedAnswer, string MissingPoints)> EvaluateAnswerAsync(
            string question, string answer);

        Task<(int OverallScore, string Strengths, string Weaknesses, string Recommendations, string SkillBreakdown, string WeakTopics)>
            GenerateSessionSummaryAsync(string topic, string level, string targetRole, List<(string Q, string A, int Score)> qa);

        Task<(List<string> Skills, string Profile)> ExtractCvProfileAsync(string cvText);
    }
}
