using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using PrepWise.API.DTOs;
using PrepWise.API.Models;
using PrepWise.API.Services;

namespace PrepWise.API.Controllers
{

    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class InterviewsController : ControllerBase
    {
        private readonly AppDbContext _context;

        private readonly IOpenAIService _aiService; // Add this field

        public InterviewsController(AppDbContext context, IOpenAIService aiService)
        {
            _context = context;
            _aiService = aiService;
        }

        
        [HttpDelete("{sessionId}")]
        public async Task<IActionResult> DeleteSession(int sessionId)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound("Session not found.");

            _context.InterviewSessions.Remove(session);
            await _context.SaveChangesAsync();

            return NoContent();
        }

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

        [HttpGet("history/{userId}")]
        public async Task<IActionResult> GetHistory(int userId)
        {
            var history = await _context.InterviewSessions
                .Where(s => s.UserId == userId)
                .Include(s => s.Questions) // This is crucial to see the scores!
                .OrderByDescending(s => s.DateCreated)
                .ToListAsync();

            if (history == null || !history.Any())
                return NotFound("No interview history found for this user.");

            return Ok(history);
        }

        [HttpPost("start")]
        public async Task<IActionResult> StartInterview([FromBody] StartInterviewRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return BadRequest("User not found.");

            string firstQuestionText;
            try
            {
                firstQuestionText = await _aiService.GenerateQuestionAsync(
                    request.InterviewType, request.Level, request.TargetRole,
                    request.InterviewGoal, request.AdditionalContext);
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
                DateCreated = DateTime.UtcNow
            };

            newSession.Questions.Add(new Question { Content = firstQuestionText, UserId = request.UserId });

            _context.InterviewSessions.Add(newSession);
            await _context.SaveChangesAsync();

            var firstQuestion = newSession.Questions.First();
            return Ok(new
            {
                SessionId = newSession.Id,
                QuestionId = firstQuestion.Id,
                FirstQuestion = firstQuestionText
            });
        }

        [HttpPost("submit-answer")]
        public async Task<IActionResult> SubmitAnswer([FromBody] SubmitAnswerRequest request)
        {
            // 1. Find the question in the database
            var question = await _context.Questions.FindAsync(request.QuestionId);
            if (question == null) return NotFound("Question not found.");

            // 2. Get AI Evaluation
            var evaluation = await _aiService.EvaluateAnswerAsync(question.Content, request.UserAnswer);

            // 3. Update the database
            question.UserAnswer = request.UserAnswer;
            question.Feedback = evaluation.Feedback;
            question.Score = evaluation.Score;

            await _context.SaveChangesAsync();

            return Ok(new
            {
                Feedback = question.Feedback,
                Score = question.Score
            });
        }

        [HttpPost("{sessionId}/next-question")]
        public async Task<IActionResult> GetNextQuestion(int sessionId)
        {
            var session = await _context.InterviewSessions
                .Include(s => s.Questions)
                .FirstOrDefaultAsync(s => s.Id == sessionId);

            if (session == null) return NotFound("Session not found.");

            // Limit to 5 questions per session for now
            if (session.Questions.Count >= 5)
                return Ok(new { Finished = true, Message = "Interview Complete!" });

            // Generate a new question from AI
            var nextQuestionText = await _aiService.GenerateQuestionAsync(
                session.InterviewType, session.Level, session.TargetRole,
                session.InterviewGoal, session.AdditionalContext);

            var newQuestion = new Question { Content = nextQuestionText, InterviewSessionId = sessionId, UserId = session.UserId };
            _context.Questions.Add(newQuestion);
            await _context.SaveChangesAsync();

            return Ok(new { Finished = false, QuestionId = newQuestion.Id, QuestionText = nextQuestionText });
        }

    }
}
