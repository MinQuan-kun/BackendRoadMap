using BackendService.Models.DTOs.Application.Responses;

namespace BackendService.Services.Interface
{
    public interface IApplicationService
    {
        Task<List<ApplicationResponseDto>> GetListAsync(CancellationToken cancellationToken);
    }
}
