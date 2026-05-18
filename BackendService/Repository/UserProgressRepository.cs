using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using MongoDB.Driver;

namespace BackendService.Repository
{
    public class UserProgressRepository : IUserProgressRepository
    {
        private readonly MongoDbContext _context;

        public UserProgressRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<UserProgress> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            var filter = Builders<UserProgress>.Filter.Eq(p => p.UserId, userId);
            return await _context.UserProgress.Find(filter).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task CreateAsync(UserProgress progress, CancellationToken cancellationToken = default)
        {
            await _context.UserProgress.InsertOneAsync(progress, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(string userId, UserProgress progress, CancellationToken cancellationToken = default)
        {
            var filter = Builders<UserProgress>.Filter.Eq(p => p.UserId, userId);
            await _context.UserProgress.ReplaceOneAsync(filter, progress, cancellationToken: cancellationToken);
        }
    }
}
