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

            var pathwayMetrics = new Dictionary<string, (int Frequency, int TotalWeight)>();
            var recommendedCourseIds = new HashSet<string>();

            foreach (var answer in request.Answers)
            {
                var question = await _careerQuizRepository.GetQuestionByIdAsync(answer.Key, cancellationToken);
                if (question == null) continue;

                var selectedTexts = answer.Value.Split(',', StringSplitOptions.RemoveEmptyEntries)
                    .Select(s => s.Trim())
                    .ToList();

                var pathwaysInQuestion = new HashSet<string>();
                var questionPathwayWeights = new Dictionary<string, int>();

                foreach (var text in selectedTexts)
                {
                    var selectedOption = question.Options.FirstOrDefault(o => o.Text == text);
                    if (selectedOption == null) continue;

                    if (selectedOption.MappingPathwayIds != null)
                    {
                        foreach (var pathwayId in selectedOption.MappingPathwayIds)
                        {
                            pathwaysInQuestion.Add(pathwayId);
                            if (!questionPathwayWeights.ContainsKey(pathwayId))
                                questionPathwayWeights[pathwayId] = 0;
                            questionPathwayWeights[pathwayId] += selectedOption.Weight;
                        }
                    }

                    if (selectedOption.MappingCourseIds != null)
                    {
                        foreach (var courseId in selectedOption.MappingCourseIds)
                            recommendedCourseIds.Add(courseId);
                    }
                }

                foreach (var pathwayId in pathwaysInQuestion)
                {
                    if (!pathwayMetrics.ContainsKey(pathwayId))
                    {
                        pathwayMetrics[pathwayId] = (0, 0);
                    }
                    var current = pathwayMetrics[pathwayId];
                    pathwayMetrics[pathwayId] = (
                        current.Frequency + 1,
                        current.TotalWeight + questionPathwayWeights[pathwayId]
                    );
                }
            }

            var topPathways = pathwayMetrics
                .OrderByDescending(x => x.Value.Frequency)
                .ThenByDescending(x => x.Value.TotalWeight)
                .Take(3)
                .Select(x => x.Key)
                .ToList();

            var result = new CareerQuizResult
            {
                UserId = userId,
                QuizId = request.QuizId,
                Answers = request.Answers,
                RecommendedPathwayIds = topPathways,
                RecommendedCourseIds = recommendedCourseIds.ToList(),
                ExplicitPreferencePathwayId = null,
                HasConflict = false,
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
