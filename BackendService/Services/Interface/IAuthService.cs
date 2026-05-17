using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.DTOs.User.Responses;
using BackendService.Models.Entities;

namespace BackendService.Services.Interface
{
    public interface IAuthService
    {
        Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default);
        string GenerateToken (User user);
        Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default);
        Task ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default);
    }
}
