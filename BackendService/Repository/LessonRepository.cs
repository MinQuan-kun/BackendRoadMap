using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using MongoDB.Driver;

namespace BackendService.Repository
{
    public class LessonRepository : ILessonRepository
    {
        private readonly MongoDbContext _context;

        public LessonRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<Lesson> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Lessons.Find(l => l.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task<LearningTask> GetTaskByIdAsync(string taskId, CancellationToken cancellationToken = default)
        {
            return await _context.Tasks.Find(t => t.Id == taskId).FirstOrDefaultAsync(cancellationToken);
        }
    }
}
