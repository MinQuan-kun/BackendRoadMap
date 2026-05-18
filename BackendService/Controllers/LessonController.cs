using Microsoft.AspNetCore.Mvc;
using BackendService.Models.DTOs.Learning;
using BackendService.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class LessonController : ControllerBase
    {
        private readonly ILessonService _lessonService;

        public LessonController(ILessonService lessonService)
        {
            _lessonService = lessonService;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<LessonDto>> GetLesson(string id, CancellationToken cancellationToken)
        {
            var lesson = await _lessonService.GetLessonDtoAsync(id, cancellationToken);
            if (lesson == null) return NotFound();

            return Ok(lesson);
        }

        [HttpPost("complete/{id}")]
        public async Task<IActionResult> CompleteLesson(string id, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var progress = await _lessonService.CompleteLessonAsync(userId, id, cancellationToken);
                return Ok(new { progress.XP, progress.Level });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }

        [HttpPost("task/complete/{taskId}")]
        public async Task<IActionResult> CompleteTask(string taskId, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                var progress = await _lessonService.CompleteTaskAsync(userId, taskId, cancellationToken);
                return Ok(new { progress.XP, progress.Level, progress.CompletedTaskIds });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
        }
    }
}