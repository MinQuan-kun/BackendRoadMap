using BackendService.Data;
using BackendService.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using System.Security.Claims;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class QuizController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public QuizController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("active")]
        public async Task<ActionResult<ActiveQuizDto>> GetActiveQuiz()
        {
            var activeQuiz = await _context.CareerQuizzes.Find(q => q.IsActive).FirstOrDefaultAsync();
            if (activeQuiz == null) return NotFound("No active quiz found.");

            var questions = await _context.CareerQuestions
                .Find(q => activeQuiz.QuestionIds.Contains(q.Id!))
                .ToListAsync();

            // Filter out weights and mapping data to prevent cheating
            var questionDtos = questions.Select(q => new QuizQuestionDto
            {
                Id = q.Id,
                Question = q.Question,
                Type = q.Type,
                Order = q.Order,
                Options = q.Options.Select(o => new QuizOptionDto
                {
                    Text = o.Text
                }).ToList()
            }).OrderBy(q => q.Order).ToList();

            return Ok(new ActiveQuizDto
            {
                Id = activeQuiz.Id,
                Title = activeQuiz.Title,
                Description = activeQuiz.Description,
                Questions = questionDtos
            });
        }

        [HttpPost("submit")]
        public async Task<ActionResult<CareerQuizResult>> SubmitQuiz([FromBody] QuizSubmissionDto request)
        {
            var activeQuiz = await _context.CareerQuizzes.Find(q => q.Id == request.QuizId).FirstOrDefaultAsync();
            if (activeQuiz == null) return NotFound("Quiz not found.");

            // Calculate scores for pathways and courses
            var pathwayScores = new Dictionary<string, int>();
            var recommendedCourseIds = new HashSet<string>();
            string? explicitPreferencePathwayId = null;
            
            // Map QuestionId -> Answer Text
            foreach (var answer in request.Answers)
            {
                var question = await _context.CareerQuestions.Find(q => q.Id == answer.Key).FirstOrDefaultAsync();
                if (question == null) continue;

                var selectedTexts = answer.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                // Check for explicit preference (Engine choice)
                if (question.Question.ToLower().Contains("học engine nào") || question.Question.ToLower().Contains("engine"))
                {
                    var primaryChoice = selectedTexts.FirstOrDefault();
                    if (primaryChoice == "Unity") explicitPreferencePathwayId = "pathway_unity";
                    else if (primaryChoice == "Unreal Engine") explicitPreferencePathwayId = "pathway_unreal";
                }

                foreach (var text in selectedTexts)
                {
                    // Find the selected option
                    var selectedOption = question.Options.FirstOrDefault(o => o.Text == text);
                    if (selectedOption != null)
                    {
                        if (selectedOption.MappingPathwayIds != null)
                        {
                            foreach (var pathwayId in selectedOption.MappingPathwayIds)
                            {
                                if (!pathwayScores.ContainsKey(pathwayId)) pathwayScores[pathwayId] = 0;
                                pathwayScores[pathwayId] += selectedOption.Weight;
                            }
                        }

                        if (selectedOption.MappingCourseIds != null)
                        {
                            foreach (var courseId in selectedOption.MappingCourseIds)
                            {
                                recommendedCourseIds.Add(courseId);
                            }
                        }
                    }
                }
            }

            // Find top pathways
            var topPathways = pathwayScores.OrderByDescending(x => x.Value).Take(3).Select(x => x.Key).ToList();
            
            bool hasConflict = false;
            if (!string.IsNullOrEmpty(explicitPreferencePathwayId) && topPathways.Any())
            {
                if (topPathways.First() != explicitPreferencePathwayId)
                {
                    hasConflict = true;
                    // Ensure the explicit choice is at least in the recommended list so they can select it
                    if (!topPathways.Contains(explicitPreferencePathwayId))
                    {
                        topPathways.Add(explicitPreferencePathwayId);
                    }
                }
            }
            
            // If we have a user context (from token)
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "anonymous";

            var result = new CareerQuizResult
            {
                UserId = userId,
                QuizId = request.QuizId,
                Answers = request.Answers,
                RecommendedPathwayIds = topPathways,
                RecommendedCourseIds = recommendedCourseIds.ToList(),
                ExplicitPreferencePathwayId = explicitPreferencePathwayId,
                HasConflict = hasConflict,
                CreatedAt = DateTime.UtcNow
            };

            await _context.CareerQuizResults.InsertOneAsync(result);

            return Ok(result);
        }

        [HttpGet("result/{id}")]
        public async Task<ActionResult<CareerQuizResult>> GetResultById(string id)
        {
            var result = await _context.CareerQuizResults.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (result == null) return NotFound();
            return Ok(result);
        }
    }

    public class ActiveQuizDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class QuizQuestionDto
    {
        public string? Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<QuizOptionDto> Options { get; set; } = new();
    }

    public class QuizOptionDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class QuizSubmissionDto
    {
        public string QuizId { get; set; } = string.Empty;
        public Dictionary<string, string> Answers { get; set; } = new();
    }
}
