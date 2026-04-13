using BackendService.Models.Entities;

namespace BackendService.Repository.Interface
{
    public interface IApplicationRepository
    {
        Task<List<Application>> GetListAsync(CancellationToken cancellationToken = default);
        Task<Application> GetByIdAsync (string Id, CancellationToken cancellationToken = default);
    }
}
