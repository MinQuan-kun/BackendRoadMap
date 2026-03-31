using BackendService.Models.Entities;

namespace BackendService.Repository.Interface
{
    public interface IJobRepository
    {
        Task<List<Job>> GetListAsync (CancellationToken cancellation = default);
    }
}
