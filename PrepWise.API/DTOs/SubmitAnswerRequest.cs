namespace PrepWise.API.DTOs
{
    public class SubmitAnswerRequest
    {
        public int QuestionId { get; set; }
        public string UserAnswer { get; set; } = string.Empty;
    }
}