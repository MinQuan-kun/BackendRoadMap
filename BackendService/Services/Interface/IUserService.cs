using BackendService.Models.DTOs.User;

namespace BackendService.Services.Interface
{
    public interface IUserService
    {
        Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken);
    }
}
