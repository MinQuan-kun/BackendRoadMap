using BackendService.Models.Entities;

namespace BackendService.Repository.Interface
{
    public interface ILessonRepository
    {
        Task<Lesson> GetByIdAsync(string id, CancellationToken cancellationToken = default);
        Task<LearningTask> GetTaskByIdAsync(string taskId, CancellationToken cancellationToken = default);
    }
}
