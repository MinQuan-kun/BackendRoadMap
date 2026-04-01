using BackendService.Models.DTOs.Application.Responses;

namespace BackendService.Services.Interface
{
    public interface IApplicationService
    {
        Task<List<ApplicationResponseDto>> GetListAsync(CancellationToken cancellationToken);
        Task<ApplicationDetailResponseDto> GetDetailAsync(string Id, CancellationToken cancellationToken);
    }
}
