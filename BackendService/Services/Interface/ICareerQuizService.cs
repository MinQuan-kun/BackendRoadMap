using BackendService.Models.DTOs.Assessment;
using BackendService.Models.Entities;

namespace BackendService.Services.Interface
{
    public interface ICareerQuizService
    {
        Task<ActiveQuizDto> GetActiveQuizDtoAsync(CancellationToken cancellationToken = default);
        Task<CareerQuizResult> SubmitQuizAsync(string userId, QuizSubmissionDto request, CancellationToken cancellationToken = default);
        Task<CareerQuizResult> GetQuizResultByIdAsync(string id, CancellationToken cancellationToken = default);
    }
}
