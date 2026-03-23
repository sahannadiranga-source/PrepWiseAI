namespace PrepWise.API.Models
{
    public class UserSkill
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public int? SessionId { get; set; }
        public string SkillName { get; set; } = string.Empty;
        public string Source { get; set; } = "CV"; // CV | Manual
    }
}
