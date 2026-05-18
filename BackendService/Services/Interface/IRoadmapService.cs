using BackendService.Models.DTOs.Roadmap;
using BackendService.Models.Entities;

namespace BackendService.Services.Interface
{
    public interface IRoadmapService
    {
        Task<(string PathwayId, string GraphId)> SaveUserRoadmapAsync(string userId, UserRoadmapRequestDto request, CancellationToken cancellationToken = default);
        Task UpdateUserRoadmapAsync(string userId, string pathwayId, UserRoadmapRequestDto request, CancellationToken cancellationToken = default);
    }
}
