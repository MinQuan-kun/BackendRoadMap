using BackendService.Models.DTOs.Job.Responses;

namespace BackendService.Services.Interface
{
    public interface IJobService
    {
        Task<List<JobListResponsedto>> GetListAsync(CancellationToken cancellationToken = default);
        Task<JobDetailResponseDto> GetByIdAsync(string Id, CancellationToken cancellationToken = default);
    }
}
