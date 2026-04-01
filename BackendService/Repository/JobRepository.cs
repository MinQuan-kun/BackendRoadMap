using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using MongoDB.Driver;

namespace BackendService.Repository
{
    public class JobRepository(MongoDbContext mongoDbContext): IJobRepository
    {
        private readonly MongoDbContext _mongoDbContext = mongoDbContext;

        public async Task<Job> GetByIdAsync(string Id, CancellationToken cancellation = default)
        {
            return await _mongoDbContext.Jobs.FindAsync(job => job.Id == Id, cancellationToken: cancellation).Result.FirstOrDefaultAsync(cancellation);
        }

        public async Task<List<Job>> GetListAsync(CancellationToken cancellation = default)
        {
            return await _mongoDbContext.Jobs
                .Find(_ => true).ToListAsync(cancellation);
        }
    }
}
