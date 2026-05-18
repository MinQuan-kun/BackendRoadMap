using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using MongoDB.Driver;

namespace BackendService.Repository
{
    public class CareerQuizRepository : ICareerQuizRepository
    {
        private readonly MongoDbContext _context;

        public CareerQuizRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<CareerQuiz> GetActiveQuizAsync(CancellationToken cancellationToken = default)
        {
            return await _context.CareerQuizzes.Find(q => q.IsActive).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<CareerQuestion>> GetQuestionsByIdsAsync(List<string> ids, CancellationToken cancellationToken = default)
        {
            return await _context.CareerQuestions
                .Find(q => ids.Contains(q.Id!))
                .ToListAsync(cancellationToken);
        }

        public async Task<CareerQuiz> GetQuizByIdAsync(string quizId, CancellationToken cancellationToken = default)
        {
            return await _context.CareerQuizzes.Find(q => q.Id == quizId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<CareerQuestion> GetQuestionByIdAsync(string questionId, CancellationToken cancellationToken = default)
        {
            return await _context.CareerQuestions.Find(q => q.Id == questionId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task CreateQuizResultAsync(CareerQuizResult result, CancellationToken cancellationToken = default)
        {
            await _context.CareerQuizResults.InsertOneAsync(result, cancellationToken: cancellationToken);
        }

        public async Task<CareerQuizResult> GetQuizResultByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.CareerQuizResults.Find(r => r.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<bool> HasCompletedQuizAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.CareerQuizResults.Find(r => r.UserId == userId).AnyAsync(cancellationToken);
        }
    }
}
