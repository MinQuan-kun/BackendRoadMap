using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.DTOs.User.Responses;

namespace BackendService.Services.Interface
{
    public interface IUserService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
        Task<ResponseUserByIdDto> GetUserByIdAsync(string id, CancellationToken cancellationToken);
        Task<ResponseUserByIdDto> UpdateProgressAsync(string userId, string nodeId, string status, CancellationToken cancellationToken);
        Task UpdateProfileAsync(string userId, UpdateProfileRequestDto request, CancellationToken cancellationToken);
        Task ChangePasswordAsync(string userId, string oldPassword, string newPassword, CancellationToken cancellationToken);
        Task DeleteAccountAsync(string userId, CancellationToken cancellationToken);
    }
}
