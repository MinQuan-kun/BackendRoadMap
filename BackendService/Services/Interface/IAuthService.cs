using BackendService.Models.DTOs.User;
using BackendService.Models.Entities;

namespace BackendService.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default);
        string GenerateToken (User user);
    }
}
