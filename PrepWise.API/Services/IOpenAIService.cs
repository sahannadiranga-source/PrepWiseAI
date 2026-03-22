namespace PrepWise.API.Services
{
    public interface IOpenAIService
    {
        Task<string> GenerateQuestionAsync(string topic, string level = "Intermediate", string targetRole = "", string interviewGoal = "", string? additionalContext = null);
        Task<(string Feedback, int Score)> EvaluateAnswerAsync(string question, string answer);
    }
}
