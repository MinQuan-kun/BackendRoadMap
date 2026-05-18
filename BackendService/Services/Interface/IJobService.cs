using BackendService.Models.DTOs.Recruitment;
using BackendService.Models.Entities;
using BackendService.Models.Entities.Recruitment;

namespace BackendService.Services.Interface
{
    public interface IJobService
    {
        Task<(List<JobResponseDto> Jobs, long Total)> GetJobsPagedAsync(string? userId, string? search, string? experience, string? skills, int page, int pageSize, CancellationToken cancellationToken = default);
        Task<JobFiltersDto> GetFiltersAsync(CancellationToken cancellationToken = default);
        Task<Job> GetJobByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<Job> CreateJobAsync(string recruiterId, Job job, CancellationToken cancellationToken = default);
        Task<Job> UpdateJobAsync(string recruiterId, string id, Job job, CancellationToken cancellationToken = default);
        Task DeleteJobAsync(string recruiterId, string id, CancellationToken cancellationToken = default);
        Task ApplyJobAsync(string userId, string jobId, CancellationToken cancellationToken = default);
        Task<List<MyApplicationDto>> GetMyApplicationsAsync(string userId, CancellationToken cancellationToken = default);
        Task<(List<MyJobPostDto> Posts, int Total)> GetMyPostsAsync(string recruiterId, CancellationToken cancellationToken = default);
        Task<(List<ApplicantDto> Applicants, int Total)> GetApplicantsAsync(string recruiterId, string jobId, CancellationToken cancellationToken = default);
        Task<JobApplication> UpdateApplicationStatusAsync(string recruiterId, string jobId, string applicationId, string status, CancellationToken cancellationToken = default);
    }
}
