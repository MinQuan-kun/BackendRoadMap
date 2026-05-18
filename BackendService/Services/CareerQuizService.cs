using BackendService.Models.DTOs.Assessment;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using BackendService.Services.Interface;

namespace BackendService.Services
{
    public class CareerQuizService : ICareerQuizService
    {
        private readonly ICareerQuizRepository _careerQuizRepository;

        public CareerQuizService(ICareerQuizRepository careerQuizRepository)
        {
            _careerQuizRepository = careerQuizRepository;
        }

        public async Task<ActiveQuizDto> GetActiveQuizDtoAsync(CancellationToken cancellationToken = default)
        {
            var activeQuiz = await _careerQuizRepository.GetActiveQuizAsync(cancellationToken);
            if (activeQuiz == null) return null;

            var questions = await _careerQuizRepository.GetQuestionsByIdsAsync(activeQuiz.QuestionIds, cancellationToken);

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

            return new ActiveQuizDto
            {
                Id = activeQuiz.Id,
                Title = activeQuiz.Title,
                Description = activeQuiz.Description,
                Questions = questionDtos
            };
        }

        public async Task<CareerQuizResult> SubmitQuizAsync(string userId, QuizSubmissionDto request, CancellationToken cancellationToken = default)
        {
            var activeQuiz = await _careerQuizRepository.GetQuizByIdAsync(request.QuizId, cancellationToken);
            if (activeQuiz == null) throw new KeyNotFoundException("Quiz not found.");

            var pathwayScores = new Dictionary<string, int>();
            var recommendedCourseIds = new HashSet<string>();
            string? explicitPreferencePathwayId = null;

            foreach (var answer in request.Answers)
            {
                var question = await _careerQuizRepository.GetQuestionByIdAsync(answer.Key, cancellationToken);
                if (question == null) continue;

                var selectedTexts = answer.Value.Split(',', StringSplitOptions.RemoveEmptyEntries).Select(s => s.Trim()).ToList();

                if (question.Question.ToLower().Contains("học engine nào") || question.Question.ToLower().Contains("engine"))
                {
                    var primaryChoice = selectedTexts.FirstOrDefault();
                    if (primaryChoice == "Unity") explicitPreferencePathwayId = "pathway_unity";
                    else if (primaryChoice == "Unreal Engine") explicitPreferencePathwayId = "pathway_unreal";
                }

                foreach (var text in selectedTexts)
                {
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

            var topPathways = pathwayScores.OrderByDescending(x => x.Value).Take(3).Select(x => x.Key).ToList();

            bool hasConflict = false;
            if (!string.IsNullOrEmpty(explicitPreferencePathwayId) && topPathways.Any())
            {
                if (topPathways.First() != explicitPreferencePathwayId)
                {
                    hasConflict = true;
                    if (!topPathways.Contains(explicitPreferencePathwayId))
                    {
                        topPathways.Add(explicitPreferencePathwayId);
                    }
                }
            }

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

            await _careerQuizRepository.CreateQuizResultAsync(result, cancellationToken);
            return result;
        }

        public async Task<CareerQuizResult> GetQuizResultByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _careerQuizRepository.GetQuizResultByIdAsync(id, cancellationToken);
        }
    }
}
