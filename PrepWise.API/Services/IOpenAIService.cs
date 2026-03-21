namespace PrepWise.API.Services
{
    public interface IOpenAIService
    {
        Task<(string Feedback, int Score)> EvaluateAnswerAsync(string question, string answer);
    }
}