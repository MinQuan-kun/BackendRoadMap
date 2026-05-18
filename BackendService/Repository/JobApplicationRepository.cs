using BackendService.Data;
using BackendService.Models.Entities.Recruitment;
using BackendService.Repository.Interface;
using MongoDB.Driver;

namespace BackendService.Repository
{
    public class JobApplicationRepository : IJobApplicationRepository
    {
        private readonly MongoDbContext _context;

        public JobApplicationRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(JobApplication application, CancellationToken cancellationToken = default)
        {
            await _context.JobApplications.InsertOneAsync(application, cancellationToken: cancellationToken);
        }

        public async Task<JobApplication> GetByJobAndUserAsync(string jobId, string userId, CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Find(a => a.JobId == jobId && a.UserId == userId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<List<JobApplication>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Find(a => a.UserId == userId)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<JobApplication>> GetByJobIdsAsync(List<string> jobIds, CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Find(a => jobIds.Contains(a.JobId))
                .ToListAsync(cancellationToken);
        }

        public async Task<List<JobApplication>> GetByJobIdAsync(string jobId, CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Find(a => a.JobId == jobId)
                .ToListAsync(cancellationToken);
        }

        public async Task<JobApplication> GetByIdAndJobIdAsync(string id, string jobId, CancellationToken cancellationToken = default)
        {
            return await _context.JobApplications
                .Find(a => a.Id == id && a.JobId == jobId)
                .FirstOrDefaultAsync(cancellationToken);
        }

        public async Task UpdateAsync(string id, JobApplication application, CancellationToken cancellationToken = default)
        {
            await _context.JobApplications.ReplaceOneAsync(a => a.Id == id, application, cancellationToken: cancellationToken);
        }

        public async Task DeleteByJobIdAsync(string jobId, CancellationToken cancellationToken = default)
        {
            await _context.JobApplications.DeleteManyAsync(a => a.JobId == jobId, cancellationToken: cancellationToken);
        }
    }
}
