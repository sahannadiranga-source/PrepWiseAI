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

        public async Task<string> GenerateQuestionAsync(string topic, string level = "Intermediate", string targetRole = "", string interviewGoal = "", string? additionalContext = null)
        {
            var context = new System.Text.StringBuilder();
            context.Append($"You are a professional technical interviewer conducting a {level}-level {topic} interview.");
            if (!string.IsNullOrWhiteSpace(targetRole))
                context.Append($" The candidate is applying for a {targetRole} position.");
            if (!string.IsNullOrWhiteSpace(interviewGoal))
                context.Append($" This is a {interviewGoal} interview.");
            if (!string.IsNullOrWhiteSpace(additionalContext))
                context.Append($" Additional context: {additionalContext}.");

            var difficulty = level switch
            {
                "Beginner" => "simple, foundational",
                "Pro" => "advanced, in-depth",
                _ => "moderately challenging"
            };

            context.Append($" Ask one {difficulty} {topic} interview question. Return only the question text, no extra commentary.");
            return await CallGemini(context.ToString());
        }

        public async Task<(string Feedback, int Score)> EvaluateAnswerAsync(string question, string answer)
        {
            var prompt = $"You are a technical interviewer. Evaluate the following answer.\n\nQuestion: {question}\nAnswer: {answer}\n\nRespond in exactly this format:\nFeedback: [your feedback here] | Score: [number 0-100]";
            var result = await CallGemini(prompt);

            var parts = result.Split('|');
            var feedback = parts[0].Replace("Feedback:", "").Trim();
            var scoreStr = parts.Length > 1 ? parts[1].Replace("Score:", "").Trim() : "0";
            int.TryParse(scoreStr, out int score);

            return (feedback, score);
        }

        private async Task<string> CallGemini(string prompt)
        {
            var apiKey = _config["Gemini:ApiKey"];
            var url = $"{BaseUrl}?key={apiKey}";

            var requestBody = new
            {
                contents = new[]
                {
                    new { parts = new[] { new { text = prompt } } }
                }
            };

            var content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json");
            var response = await _httpClient.PostAsync(url, content);
            var responseString = await response.Content.ReadAsStringAsync();

            if (!response.IsSuccessStatusCode)
                throw new Exception($"Gemini error {(int)response.StatusCode}: {responseString}");

            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement
                .GetProperty("candidates")[0]
                .GetProperty("content")
                .GetProperty("parts")[0]
                .GetProperty("text")
                .GetString() ?? "Error generating response.";
        }
    }
}
