using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using MongoDB.Driver;

namespace BackendService.Repository
{
    public class PathwayRepository : IPathwayRepository
    {
        private readonly MongoDbContext _context;

        public PathwayRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task CreatePathwayAsync(Pathway pathway, CancellationToken cancellationToken = default)
        {
            await _context.Pathways.InsertOneAsync(pathway, cancellationToken: cancellationToken);
        }

        public async Task<Pathway?> GetPathwayByIdAndUserAsync(string id, string userId, CancellationToken cancellationToken = default)
        {
            return await _context.Pathways.Find(p => p.Id == id && p.CreatedBy == userId).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdatePathwayTitleAsync(string id, string title, CancellationToken cancellationToken = default)
        {
            await _context.Pathways.UpdateOneAsync(
                p => p.Id == id,
                Builders<Pathway>.Update.Set(p => p.Title, title),
                cancellationToken: cancellationToken
            );
        }
    }
}
