namespace PrepWise.API.DTOs
{
    public class StartInterviewRequest
    {
        public int UserId { get; set; }
        public string InterviewType { get; set; } = "Technical"; // e.g., ".NET" or "Behavioral"
    }
}