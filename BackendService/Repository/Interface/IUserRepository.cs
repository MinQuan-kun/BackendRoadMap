using BackendService.Models.Entities;

namespace BackendService.Repository.Interface
{
    public interface IUserRepository
    {
        Task<User> CreateAsync(User user, CancellationToken cancellationToken = default);
        Task<User> GetByEmailAsync (string email, CancellationToken cancellationToken = default);
        Task <User> GetByUserNameAsync(string userName, CancellationToken cancellationToken = default);
    }
}
