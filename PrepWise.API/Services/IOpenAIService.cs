namespace PrepWise.API.Services
{
    public interface IOpenAIService
    {
      
        Task<string> GenerateQuestionAsync(string topic);

       
        Task<(string Feedback, int Score)> EvaluateAnswerAsync(string question, string answer);
    }
}