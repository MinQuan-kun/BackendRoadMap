using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.DTOs.User.Responses;

namespace BackendService.Services.Interface
{
    public interface IUserService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
        Task<ResponseUserByIdDto> GetUserByIdAsync(string id, CancellationToken cancellationToken);
    }
}
