using System.Text;
using System.Text.Json;

namespace PrepWise.API.Services
{
    public class OpenAIService : IOpenAIService
    {
        private readonly IConfiguration _config;
        private readonly HttpClient _httpClient;

        public OpenAIService(IConfiguration config, HttpClient httpClient)
        {
            _config = config;
            _httpClient = httpClient;
        }

        public async Task<string> GenerateQuestionAsync(string topic)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            var requestBody = new
            {
                model = "gpt-3.5-turbo", // or gpt-4
                messages = new[]
                {
                    new { role = "system", content = "You are a professional technical interviewer." },
                    new { role = "user", content = $"Give me one challenging {topic} interview question. Return only the question text." }
                },
                max_tokens = 100
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Headers = { { "Authorization", $"Bearer {apiKey}" } },
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            return doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "Error generating question.";
        }

        public async Task<(string Feedback, int Score)> EvaluateAnswerAsync(string question, string answer)
        {
            var apiKey = _config["OpenAI:ApiKey"];
            var requestBody = new
            {
                model = "gpt-4o-mini",
                messages = new[]
                {
            new { role = "system", content = "You are a technical interviewer. Evaluate the user's answer. Provide helpful feedback and a score out of 100. Format your response exactly like this: Feedback: [Your Feedback] | Score: [Number]" },
            new { role = "user", content = $"Question: {question}\nUser Answer: {answer}" }
        },
                max_tokens = 200
            };

            var request = new HttpRequestMessage(HttpMethod.Post, "https://api.openai.com/v1/chat/completions")
            {
                Headers = { { "Authorization", $"Bearer {apiKey}" } },
                Content = new StringContent(JsonSerializer.Serialize(requestBody), Encoding.UTF8, "application/json")
            };

            var response = await _httpClient.SendAsync(request);
            var responseString = await response.Content.ReadAsStringAsync();

            using var doc = JsonDocument.Parse(responseString);
            var aiResult = doc.RootElement.GetProperty("choices")[0].GetProperty("message").GetProperty("content").GetString() ?? "";

            // Simple parsing logic
            var parts = aiResult.Split('|');
            var feedback = parts[0].Replace("Feedback:", "").Trim();
            var scoreStr = parts.Length > 1 ? parts[1].Replace("Score:", "").Trim() : "0";
            int.TryParse(scoreStr, out int score);

            return (feedback, score);
        }
    }
}