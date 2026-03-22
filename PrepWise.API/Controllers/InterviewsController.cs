using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrepWise.API.DTOs;
using PrepWise.API.Models;
using PrepWise.API.Services;
using System.Security.Claims;
using System.Text;
using System.Text.Json;
using UglyToad.PdfPig;

namespace PrepWise.API.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewsController : ControllerBase
    {
        private readonly AppDbContext _context;
        private readonly IOpenAIService _aiService;

        public InterviewsController(AppDbContext context, IOpenAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        // ── History ──────────────────────────────────────────────────────────
        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            var history = await _context.InterviewSessions
                .Where(s => s.UserId == userId)
                .Include(s => s.Questions)
                .Include(s => s.Analytics)
                .OrderByDescending(s => s.DateCreated)
                .ToListAsync();
            return Ok(history);
        }

        // ── Start ─────────────────────────────────────────────────────────────
        [HttpPost("start")]
        public async Task<IActionResult> StartInterview([FromBody] StartInterviewRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return BadRequest("User not found.");

            var cvSkills = request.CvSkills;

            string firstQuestionText;
            try
            {
                firstQuestionText = await _aiService.GenerateQuestionAsync(
                    request.InterviewType, request.Level, request.TargetRole,
                    request.InterviewGoal, request.AdditionalContext, cvSkills,
                    "Conceptual", GetDifficultyForLevel(request.Level, 1));
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { error = "AI service failed", detail = ex.Message });
            }

            var newSession = new InterviewSession
            {
                UserId = request.UserId,
                InterviewType = request.InterviewType,
                Level = request.Level,
                TargetRole = request.TargetRole,
                InterviewGoal = request.InterviewGoal,
                AdditionalContext = request.AdditionalContext,
                ExperienceLevel = request.ExperienceLevel,
                Mode = request.Mode,
                CvExtractedSkills = request.CvSkills != null ? JsonSerializer.Serialize(request.CvSkills) : null,
                DateCreated = DateTime.UtcNow
            };

            newSession.Questions.Add(new Question
            {
                Content = firstQuestionText,
                UserId = request.UserId,
                QuestionType = "Conceptual",
                Difficulty = GetDifficultyForLevel(request.Level, 1)
            });

            _context.InterviewSessions.Add(newSession);
            await _context.SaveChangesAsync();

            var firstQuestion = newSession.Questions.First();
            return Ok(new { SessionId = newSession.Id, QuestionId = firstQuestion.Id, FirstQuestion = firstQuestionText });
        }

        // ── Submit Answer ─────────────────────────────────────────────────────
        [HttpPost("submit-answer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequest request)
        {
            var question = await _context.Questions.FindAsync(request.QuestionId);
            if (question == null) return NotFound("Question not found.");

            var (feedback, score, suggested, missing) = await _aiService.EvaluateAnswerAsync(question.Content, request.UserAnswer);

            question.UserAnswer = request.UserAnswer;
            question.Feedback = feedback;
            question.Score = score;
            question.SuggestedAnswer = suggested;
            question.MissingPoints = missing;

            await _context.SaveChangesAsync();

            return Ok(new { Feedback = feedback, Score = score, SuggestedAnswer = suggested, MissingPoints = missing });
        }

        // ── Next Question ─────────────────────────────────────────────────────
        [HttpPost("{sessionId}/next-question")]
        public async Task<IActionResult> GetNextQuestion(int sessionId)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound("Session not found.");
            if (session.Questions.Count >= 5)
                return Ok(new { Finished = true, Message = "Interview Complete!" });

            var qNum = session.Questions.Count + 1;
            var questionType = GetQuestionType(qNum);
            var difficulty = GetAdaptiveDifficulty(session);
            var cvSkills = session.CvExtractedSkills != null
                ? JsonSerializer.Deserialize<List<string>>(session.CvExtractedSkills)
                : null;

            var nextQuestionText = await _aiService.GenerateQuestionAsync(
                session.InterviewType, session.Level, session.TargetRole,
                session.InterviewGoal, session.AdditionalContext, cvSkills, questionType, difficulty);

            var newQuestion = new Question
            {
                Content = nextQuestionText,
                InterviewSessionId = sessionId,
                UserId = session.UserId,
                QuestionType = questionType,
                Difficulty = difficulty
            };
            _context.Questions.Add(newQuestion);
            await _context.SaveChangesAsync();

            return Ok(new { Finished = false, QuestionId = newQuestion.Id, QuestionText = nextQuestionText, QuestionType = questionType, Difficulty = difficulty });
        }

        // ── Session Summary ───────────────────────────────────────────────────
        [HttpGet("summary/{sessionId}")]
        public async Task<IActionResult> GetSummary(int sessionId)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Questions)
                .Include(s => s.Analytics)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound();

            // Return cached analytics if already generated
            if (session.Analytics != null)
                return Ok(session.Analytics);

            var answeredQs = session.Questions.Where(q => !string.IsNullOrEmpty(q.UserAnswer)).ToList();
            if (!answeredQs.Any())
                return BadRequest("No answered questions to summarize.");

            var qaList = answeredQs.Select(q => (q.Content, q.UserAnswer ?? "", q.Score)).ToList();

            try
            {
                var (overall, strengths, weaknesses, recommendations, skillBreakdown) =
                    await _aiService.GenerateSessionSummaryAsync(session.InterviewType, session.Level, session.TargetRole, qaList);

                var analytics = new SessionAnalytics
                {
                    SessionId = sessionId,
                    OverallScore = overall,
                    Strengths = strengths,
                    Weaknesses = weaknesses,
                    Recommendations = recommendations,
                    SkillBreakdown = skillBreakdown,
                    GeneratedAt = DateTime.UtcNow
                };

                _context.SessionAnalytics.Add(analytics);
                await _context.SaveChangesAsync();

                return Ok(analytics);
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { error = "AI summary failed", detail = ex.Message });
            }
        }

        // ── Upload CV ─────────────────────────────────────────────────────────
        [HttpPost("upload-cv")]
        public async Task<IActionResult> UploadCv([FromForm] IFormFile file, [FromForm] int userId)
        {
            if (file == null || file.Length == 0)
                return BadRequest("No file uploaded.");

            string cvText;
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (ext == ".pdf")
            {
                // Use PdfPig to extract text from PDF
                using var ms = new MemoryStream();
                await file.CopyToAsync(ms);
                ms.Position = 0;
                var sb = new StringBuilder();
                using var pdf = PdfDocument.Open(ms.ToArray());
                foreach (var page in pdf.GetPages())
                    sb.AppendLine(page.Text);
                cvText = sb.ToString();
            }
            else
            {
                // Plain text file
                using var stream = file.OpenReadStream();
                using var reader = new StreamReader(stream, Encoding.UTF8, detectEncodingFromByteOrderMarks: true);
                cvText = await reader.ReadToEndAsync();
            }

            if (string.IsNullOrWhiteSpace(cvText))
                return BadRequest("Could not extract text from file.");

            try
            {
                var skills = await _aiService.ExtractSkillsFromCvAsync(cvText);

                var existing = _context.UserSkills.Where(s => s.UserId == userId && s.Source == "CV");
                _context.UserSkills.RemoveRange(existing);
                _context.UserSkills.AddRange(skills.Select(s => new UserSkill { UserId = userId, SkillName = s, Source = "CV" }));
                await _context.SaveChangesAsync();

                return Ok(new { Skills = skills });
            }
            catch (Exception ex)
            {
                return StatusCode(502, new { error = "Skill extraction failed", detail = ex.Message });
            }
        }

        // ── Delete ────────────────────────────────────────────────────────────
        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Questions)
                .Include(s => s.Analytics)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound("Session not found.");
            _context.InterviewSessions.Remove(session);
            await _context.SaveChangesAsync();
            return NoContent();
        }

        // ── Test AI ───────────────────────────────────────────────────────────
        [AllowAnonymous]
        [HttpGet("test-ai")]
        public async Task<IActionResult> TestAI()
        {
            try
            {
                var question = await _aiService.GenerateQuestionAsync(".NET");
                return Ok(new { success = true, question });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { success = false, error = ex.Message });
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private static string GetQuestionType(int questionNumber) => questionNumber switch
        {
            1 or 2 => "Conceptual",
            3 => "Scenario",
            4 => "ProblemSolving",
            _ => "Behavioral"
        };

        private static string GetDifficultyForLevel(string level, int questionNumber) => level switch
        {
            "Beginner" => "Easy",
            "Pro" => questionNumber <= 2 ? "Medium" : "Hard",
            _ => questionNumber <= 2 ? "Easy" : "Medium"
        };

        private static string GetAdaptiveDifficulty(InterviewSession session)
        {
            var answered = session.Questions.Where(q => q.Score > 0).ToList();
            if (!answered.Any()) return "Medium";
            var avgScore = answered.Average(q => q.Score);
            return avgScore >= 70 ? "Hard" : avgScore < 40 ? "Easy" : "Medium";
        }
    }
}
