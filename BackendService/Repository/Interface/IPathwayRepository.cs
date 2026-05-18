using BackendService.Models.Entities;

namespace BackendService.Repository.Interface
{
    public interface IPathwayRepository
    {
        Task CreatePathwayAsync(Pathway pathway, CancellationToken cancellationToken = default);
        Task<Pathway?> GetPathwayByIdAndUserAsync(string id, string userId, CancellationToken cancellationToken = default);
        Task UpdatePathwayTitleAsync(string id, string title, CancellationToken cancellationToken = default);
    }
}
