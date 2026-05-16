using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BackendService.Data;
using BackendService.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BackendService.Models.DTOs.Learning;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LessonController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public LessonController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LessonDto>> GetLesson(string id)
        {
            var lesson = await _context.Lessons.Find(l => l.Id == id).FirstOrDefaultAsync();
            if (lesson == null) return NotFound();

            return Ok(new LessonDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Difficulty = lesson.Difficulty,
                EstimatedMinutes = lesson.EstimatedMinutes,
                TaskIds = lesson.TaskIds,
                QuizIds = lesson.QuizIds,
                XPReward = lesson.XPReward
            });
        }

        [HttpPost("complete/{id}")]
        public async Task<IActionResult> CompleteLesson(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var lesson = await _context.Lessons.Find(l => l.Id == id).FirstOrDefaultAsync();
            if (lesson == null) return NotFound();

            var filter = Builders<UserProgress>.Filter.Eq(p => p.UserId, userId);
            var progress = await _context.UserProgress.Find(filter).FirstOrDefaultAsync();

            if (progress == null)
            {
                progress = new UserProgress { UserId = userId };
                await _context.UserProgress.InsertOneAsync(progress);
            }

            if (!progress.CompletedLessonIds.Contains(id))
            {
                progress.CompletedLessonIds.Add(id);
                progress.XP += lesson.XPReward;
                progress.LastActivity = DateTime.UtcNow;
                
                progress.Level = (progress.XP / 1000) + 1;

                await _context.UserProgress.ReplaceOneAsync(filter, progress);
            }

            return Ok(new { progress.XP, progress.Level });
        }

        [HttpPost("task/complete/{taskId}")]
        public async Task<IActionResult> CompleteTask(string taskId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var task = await _context.Tasks.Find(t => t.Id == taskId).FirstOrDefaultAsync();
            if (task == null) return NotFound();

            var filter = Builders<UserProgress>.Filter.Eq(p => p.UserId, userId);
            var progress = await _context.UserProgress.Find(filter).FirstOrDefaultAsync();

            if (progress == null)
            {
                progress = new UserProgress { UserId = userId };
                await _context.UserProgress.InsertOneAsync(progress);
            }

            if (!progress.CompletedTaskIds.Contains(taskId))
            {
                progress.CompletedTaskIds.Add(taskId);
                progress.XP += task.XPReward;
                progress.LastActivity = DateTime.UtcNow;
                
                progress.Level = (progress.XP / 1000) + 1;

                await _context.UserProgress.ReplaceOneAsync(filter, progress);
            }

            return Ok(new { progress.XP, progress.Level, progress.CompletedTaskIds });
        }
    }
}