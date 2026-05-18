using BackendService.Models.DTOs.Assessment;
using BackendService.Models.Entities;
using BackendService.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly ICareerQuizService _careerQuizService;

        public QuizController(ICareerQuizService careerQuizService)
        {
            _careerQuizService = careerQuizService;
        }

        [HttpGet("active")]
        public async Task<ActionResult<ActiveQuizDto>> GetActiveQuiz(CancellationToken cancellationToken)
        {
            var activeQuiz = await _careerQuizService.GetActiveQuizDtoAsync(cancellationToken);
            if (activeQuiz == null) return NotFound("No active quiz found.");

            return Ok(activeQuiz);
        }

        [Authorize]
        [HttpPost("submit")]
        public async Task<ActionResult<CareerQuizResult>> SubmitQuiz([FromBody] QuizSubmissionDto request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

            try
            {
                var result = await _careerQuizService.SubmitQuizAsync(userId, request, cancellationToken);
                return Ok(result);
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        [HttpGet("result/{id}")]
        public async Task<ActionResult<CareerQuizResult>> GetResultById(string id, CancellationToken cancellationToken)
        {
            var result = await _careerQuizService.GetQuizResultByIdAsync(id, cancellationToken);
            if (result == null) return NotFound();
            return Ok(result);
        }
    }
}
