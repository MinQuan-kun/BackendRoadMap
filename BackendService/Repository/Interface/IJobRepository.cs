using BackendService.Models.Entities.Recruitment;

namespace BackendService.Repository.Interface
{
    public interface IJobRepository
    {
        Task<(List<Job> Jobs, long Total)> GetJobsAsync(string? search, string? experienceLevel, List<string>? skills, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<List<string>> GetDistinctLocationsAsync(CancellationToken cancellationToken = default);
        Task<List<string>> GetDistinctExperienceLevelsAsync(CancellationToken cancellationToken = default);
        Task<List<string>> GetAllSkillTagsAsync(CancellationToken cancellationToken = default);
        Task<List<Job>> GetByRecruiterIdAsync(string recruiterId, CancellationToken cancellationToken = default);
        Task<Job> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task CreateAsync(Job job, CancellationToken cancellationToken = default);
        Task UpdateAsync(string id, Job job, CancellationToken cancellationToken = default);
        Task DeleteAsync(string id, CancellationToken cancellationToken = default);
    }
}
