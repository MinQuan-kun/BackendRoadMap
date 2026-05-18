using BackendService.Models.DTOs.Learning;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using BackendService.Services.Interface;

namespace BackendService.Services
{
    public class LessonService : ILessonService
    {
        private readonly ILessonRepository _lessonRepository;
        private readonly IUserProgressRepository _userProgressRepository;

        public LessonService(ILessonRepository lessonRepository, IUserProgressRepository userProgressRepository)
        {
            _lessonRepository = lessonRepository;
            _userProgressRepository = userProgressRepository;
        }

        public async Task<LessonDto> GetLessonDtoAsync(string id, CancellationToken cancellationToken = default)
        {
            var lesson = await _lessonRepository.GetByIdAsync(id, cancellationToken);
            if (lesson == null) return null;

            return new LessonDto
            {
                Id = lesson.Id,
                Title = lesson.Title,
                Description = lesson.Description,
                Difficulty = lesson.Difficulty,
                EstimatedMinutes = lesson.EstimatedMinutes,
                TaskIds = lesson.TaskIds,
                QuizIds = lesson.QuizIds,
                XPReward = lesson.XPReward
            };
        }

        public async Task<UserProgress> CompleteLessonAsync(string userId, string lessonId, CancellationToken cancellationToken = default)
        {
            var lesson = await _lessonRepository.GetByIdAsync(lessonId, cancellationToken);
            if (lesson == null) throw new KeyNotFoundException("Lesson not found.");

            var progress = await _userProgressRepository.GetByUserIdAsync(userId, cancellationToken);

            if (progress == null)
            {
                progress = new UserProgress { UserId = userId };
                await _userProgressRepository.CreateAsync(progress, cancellationToken);
            }

            if (!progress.CompletedLessonIds.Contains(lessonId))
            {
                progress.CompletedLessonIds.Add(lessonId);
                progress.XP += lesson.XPReward;
                progress.LastActivity = DateTime.UtcNow;
                progress.Level = (progress.XP / 1000) + 1;

                await _userProgressRepository.UpdateAsync(userId, progress, cancellationToken);
            }

            return progress;
        }

        public async Task<UserProgress> CompleteTaskAsync(string userId, string taskId, CancellationToken cancellationToken = default)
        {
            var task = await _lessonRepository.GetTaskByIdAsync(taskId, cancellationToken);
            if (task == null) throw new KeyNotFoundException("Task not found.");

            var progress = await _userProgressRepository.GetByUserIdAsync(userId, cancellationToken);

            if (progress == null)
            {
                progress = new UserProgress { UserId = userId };
                await _userProgressRepository.CreateAsync(progress, cancellationToken);
            }

            if (!progress.CompletedTaskIds.Contains(taskId))
            {
                progress.CompletedTaskIds.Add(taskId);
                progress.XP += task.XPReward;
                progress.LastActivity = DateTime.UtcNow;
                progress.Level = (progress.XP / 1000) + 1;

                await _userProgressRepository.UpdateAsync(userId, progress, cancellationToken);
            }

            return progress;
        }
    }
}
