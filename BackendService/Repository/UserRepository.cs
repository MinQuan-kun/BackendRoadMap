using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using MongoDB.Driver;

namespace BackendService.Repository
{
    public class UserRepository(MongoDbContext dbContext) : IUserRepository
    {
        private readonly MongoDbContext _dbContext = dbContext;
        public async Task<User> CreateAsync(User user, CancellationToken cancellationToken = default)
        {
            await _dbContext.Users.InsertOneAsync(user, cancellationToken: cancellationToken);
            return user;
        }

        public async Task<User> GetByEmailAsync(string email, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                .Find(u => u.Email == email)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<User> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Users
                            .Find(u => u.UserName == userName)
                            .FirstOrDefaultAsync(cancellationToken);
        }
    }
}
