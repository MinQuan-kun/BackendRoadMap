using BackendService.Models.Entities;

namespace BackendService.Repository.Interface
{
    public interface ICareerQuizRepository
    {
        Task<CareerQuiz> GetActiveQuizAsync(CancellationToken cancellationToken = default);
        Task<List<CareerQuestion>> GetQuestionsByIdsAsync(List<string> ids, CancellationToken cancellationToken = default);
        Task<CareerQuiz> GetQuizByIdAsync(string quizId, CancellationToken cancellationToken = default);
        Task<CareerQuestion> GetQuestionByIdAsync(string questionId, CancellationToken cancellationToken = default);
        Task CreateQuizResultAsync(CareerQuizResult result, CancellationToken cancellationToken = default);
        Task<CareerQuizResult> GetQuizResultByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<bool> HasCompletedQuizAsync(string userId, CancellationToken cancellationToken = default);
    }
}
