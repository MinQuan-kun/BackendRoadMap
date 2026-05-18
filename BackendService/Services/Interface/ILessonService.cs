using BackendService.Models.DTOs.Learning;
using BackendService.Models.Entities;

namespace BackendService.Services.Interface
{
    public interface ILessonService
    {
        Task<LessonDto> GetLessonDtoAsync(string id, CancellationToken cancellationToken = default);
        Task<UserProgress> CompleteLessonAsync(string userId, string lessonId, CancellationToken cancellationToken = default);
        Task<UserProgress> CompleteTaskAsync(string userId, string taskId, CancellationToken cancellationToken = default);
    }
}
