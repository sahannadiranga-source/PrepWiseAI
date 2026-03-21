namespace PrepWise.API.Models
{
    public class InterviewSession
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string InterviewType { get; set; } = "Technical"; // e.g., HR or .NET
        public DateTime DateCreated { get; set; } = DateTime.UtcNow;
        public List<Question> Questions { get; set; } = new();
    }
}
