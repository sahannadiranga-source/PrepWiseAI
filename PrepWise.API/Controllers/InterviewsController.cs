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

        [HttpPost("start")]
        public async Task<IActionResult> StartInterview([FromBody] StartInterviewRequest request)
        {
            var user = await _context.Users.FindAsync(request.UserId);
            if (user == null) return BadRequest("User not found.");

            // 1. Generate the first question via AI
            var firstQuestionText = await _aiService.GenerateQuestionAsync(request.InterviewType);

            // 2. Create the Session
            var newSession = new InterviewSession
            {
                UserId = request.UserId,
                InterviewType = request.InterviewType,
                DateCreated = DateTime.UtcNow
            };

            // 3. Add the first question to the session
            newSession.Questions.Add(new Question { Content = firstQuestionText });

            _context.InterviewSessions.Add(newSession);
            await _context.SaveChangesAsync();

            return Ok(new
            {
                SessionId = newSession.Id,
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

    }
}
