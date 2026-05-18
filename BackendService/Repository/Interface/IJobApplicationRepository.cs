using BackendService.Models.Entities.Recruitment;

namespace BackendService.Repository.Interface
{
    public interface IJobApplicationRepository
    {
        Task CreateAsync(JobApplication application, CancellationToken cancellationToken = default);
        Task<JobApplication> GetByJobAndUserAsync(string jobId, string userId, CancellationToken cancellationToken = default);
        Task<List<JobApplication>> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task<List<JobApplication>> GetByJobIdsAsync(List<string> jobIds, CancellationToken cancellationToken = default);
        Task<List<JobApplication>> GetByJobIdAsync(string jobId, CancellationToken cancellationToken = default);
        Task<JobApplication> GetByIdAndJobIdAsync(string id, string jobId, CancellationToken cancellationToken = default);
        Task UpdateAsync(string id, JobApplication application, CancellationToken cancellationToken = default);
        Task DeleteByJobIdAsync(string jobId, CancellationToken cancellationToken = default);
    }
}
