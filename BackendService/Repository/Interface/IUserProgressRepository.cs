using BackendService.Models.Entities;

namespace BackendService.Repository.Interface
{
    public interface IUserProgressRepository
    {
        Task<UserProgress> GetByUserIdAsync(string userId, CancellationToken cancellationToken = default);
        Task CreateAsync(UserProgress progress, CancellationToken cancellationToken = default);
        Task UpdateAsync(string userId, UserProgress progress, CancellationToken cancellationToken = default);
    }
}
