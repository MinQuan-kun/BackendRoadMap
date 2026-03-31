using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using MongoDB.Driver;


namespace BackendService.Repository
{
    public class ApplicationRepository(MongoDbContext mongoDbContext) : IApplicationRepository
    {
        private readonly MongoDbContext _mongoDbContext = mongoDbContext;

        public async Task<List<Application>> GetListAsync(CancellationToken cancellationToken = default)
        {
            return await _mongoDbContext.Applications
                .Find(_ => true)
                .ToListAsync(cancellationToken);
        }
    }
}
