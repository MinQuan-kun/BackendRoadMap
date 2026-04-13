using BackendService.Data;
using BackendService.Models.DTOs.Question;
using BackendService.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/quizs")]
    public class QuizController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public QuizController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("questions")]
        public async Task<ActionResult<List<QuestionBank>>> GetQuestions()
        {
            var questions = await _context.Questions.Find(_ => true).ToListAsync();
            return Ok(questions);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmissionDto submission)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, submission.UserId);

            var update = Builders<User>.Update.Set(u => u.InterestedNodes, submission.SelectedNodeIds);

            if (submission.SkipBasics)
            {
                var basicNodeIds = await _context.Nodes
                    .Find(n => n.Category == "Language" || n.Category == "Language Syntax")
                    .Project(n => n.Id)
                    .ToListAsync();

                update = update.AddToSetEach(u => u.CompletedNodes, basicNodeIds);
            }

            var result = await _context.Users.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0) return NotFound("Không tìm thấy User!");

            return Ok(new { message = "Lộ trình của bạn đã được khởi tạo!" });
        }
    }
}