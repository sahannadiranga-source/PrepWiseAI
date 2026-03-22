using System.Text;
using System.Text.Json;

namespace PrepWise.API.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;
        private const string BaseUrl = "https://generativelanguage.googleapis.com/v1beta/models/gemini-2.5-flash:generateContent";

        public OpenAIService(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        public async Task<string> GenerateQuestionAsync(
            string topic, string level = "Intermediate", string targetRole = "",
            string interviewGoal = "", string? additionalContext = null,
            List<string>? cvSkills = null, string questionType = "Conceptual", string difficulty = "Medium",
            string? cvProfile = null, List<(string Q, string A)>? previousQA = null)
        {
            var sb = new StringBuilder();
            sb.Append($"You are a professional technical interviewer. Generate a single {difficulty}-difficulty {questionType} interview question about {topic}.");

            if (!string.IsNullOrWhiteSpace(targetRole))
                sb.Append($" The candidate is applying for a {targetRole} role.");
            if (!string.IsNullOrWhiteSpace(interviewGoal))
                sb.Append($" This is a {interviewGoal}-level interview.");
            if (!string.IsNullOrWhiteSpace(additionalContext))
                sb.Append($" Context: {additionalContext}.");

            // CV profile gives full context: projects, experience, skills
            if (!string.IsNullOrWhiteSpace(cvProfile))
            {
                sb.Append($"\n\nCANDIDATE CV PROFILE:\n{cvProfile}");
                sb.Append("\nUse this profile to ask project-specific or experience-specific questions when relevant.");
                sb.Append(" For example, if the CV mentions a specific project, ask about its architecture, challenges, or decisions made.");
            }
            else if (cvSkills?.Any() == true)
            {
                sb.Append($" The candidate has skills in: {string.Join(", ", cvSkills)}. Tailor the question to their background.");
            }

            // Previous Q&A context for follow-up awareness
            if (previousQA?.Any() == true)
            {
                sb.Append("\n\nPREVIOUS INTERVIEW Q&A (for context — avoid repeating topics, and ask follow-ups if the candidate mentioned specific technologies):");
                foreach (var (q, a) in previousQA.TakeLast(3))
                    sb.Append($"\nQ: {q}\nA: {a}");
                sb.Append("\nIf the candidate mentioned a specific tool, framework, or project in their answers above, consider asking a follow-up about it.");
            }

            sb.Append("\n\nReturn ONLY the question text. No preamble, no numbering, no extra commentary.");
            return await CallGemini(sb.ToString());
        }

        public async Task<(string Feedback, int Score, string SuggestedAnswer, string MissingPoints)> EvaluateAnswerAsync(
            string question, string answer)
        {
            var prompt = $@"You are a technical interviewer. Evaluate the following answer strictly.

Question: {question}
Answer: {answer}

Respond in EXACTLY this format (use | as separator, no newlines between fields):
Feedback: [concise feedback] | Score: [0-100] | SuggestedAnswer: [ideal answer in 2-3 sentences] | MissingPoints: [key points the candidate missed, comma separated]";

            var result = await CallGemini(prompt);
            var parts = result.Split('|');

            string Get(string key) => parts
                .FirstOrDefault(p => p.TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase))
                ?.Substring(parts.First(p => p.TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase)).IndexOf(':') + 1)
                .Trim() ?? "";

            int.TryParse(Get("Score"), out int score);
            return (Get("Feedback"), score, Get("SuggestedAnswer"), Get("MissingPoints"));
        }

        public async Task<(int OverallScore, string Strengths, string Weaknesses, string Recommendations, string SkillBreakdown, string WeakTopics)>
            GenerateSessionSummaryAsync(string topic, string level, string targetRole, List<(string Q, string A, int Score)> qa)
        {
            var qaSummary = string.Join("\n", qa.Select((x, i) => $"Q{i + 1} (Score:{x.Score}): {x.Q}\nA: {x.A}"));
            var prompt = $@"You are a senior technical interviewer. Analyze this {level} {topic} interview for a {targetRole} candidate.

{qaSummary}

Respond in EXACTLY this format (use | as separator):
OverallScore: [0-100] | Strengths: [2-3 key strengths] | Weaknesses: [2-3 areas to improve] | Recommendations: [actionable advice] | SkillBreakdown: [topic:score pairs like ""Fundamentals:75,Problem Solving:60""] | WeakTopics: [comma-separated list of weak topic names the candidate should retake, e.g. ""SQL Joins,Docker Networking""]";

            var result = await CallGemini(prompt);
            var parts = result.Split('|');

            string Get(string key) => parts
                .FirstOrDefault(p => p.TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase))
                ?.Substring(parts.First(p => p.TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase)).IndexOf(':') + 1)
                .Trim() ?? "";

            int.TryParse(Get("OverallScore"), out int overall);
            return (overall, Get("Strengths"), Get("Weaknesses"), Get("Recommendations"), Get("SkillBreakdown"), Get("WeakTopics"));
        }

        public async Task<(List<string> Skills, string Profile)> ExtractCvProfileAsync(string cvText)
        {
            var truncated = cvText[..Math.Min(cvText.Length, 4000)];
            var prompt = $@"Analyze this CV and extract structured information.

CV TEXT:
{truncated}

Respond in EXACTLY this format (use | as separator):
Skills: [comma-separated technical skills list] | Profile: [2-4 sentence summary covering: candidate's experience level, key technologies, notable projects with brief descriptions, and career focus. Be specific about project names and what they built.]";

            var result = await CallGemini(prompt);
            var parts = result.Split('|');

            string Get(string key) => parts
                .FirstOrDefault(p => p.TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase))
                ?.Substring(parts.First(p => p.TrimStart().StartsWith(key, StringComparison.OrdinalIgnoreCase)).IndexOf(':') + 1)
                .Trim() ?? "";

            var skillsRaw = Get("Skills");
            var profile = Get("Profile");
            var skills = skillsRaw.Split(',').Select(s => s.Trim()).Where(s => !string.IsNullOrEmpty(s)).Take(20).ToList();

            return (skills, profile);
        }

        private async Task<string> CallGemini(string prompt)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var url = $"{BaseUrl}?key={apiKey}";
            var requestBody = new { contents = new[] { new { parts = new[] { new { text = prompt } } } } };
            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini error {(int)response.StatusCode}: {responseString}");
            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("candidates")[0].GetProperty("content")
                .GetProperty("parts")[0].GetProperty("text").GetString() ?? "";
        }
    }
}
