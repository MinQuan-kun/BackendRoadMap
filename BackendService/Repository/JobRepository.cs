using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using MongoDB.Driver;

namespace BackendService.Repository
{
    public class JobRepository(MongoDbContext mongoDbContext): IJobRepository
    {
        private readonly MongoDbContext _mongoDbContext = mongoDbContext;

        public async Task<List<Job>> GetListAsync(CancellationToken cancellation = default)
        {
            return await _mongoDbContext.Jobs
                .Find(_ => true).ToListAsync(cancellation);

        }
    }
}
