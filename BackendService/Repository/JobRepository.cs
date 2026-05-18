using BackendService.Data;
using BackendService.Models.Entities.Recruitment;
using BackendService.Repository.Interface;
using MongoDB.Driver;

namespace BackendService.Repository
{
    public class JobRepository : IJobRepository
    {
        private readonly MongoDbContext _context;

        public JobRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task<(List<Job> Jobs, long Total)> GetJobsAsync(string? search, string? experienceLevel, List<string>? skills, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var filterBuilder = Builders<Job>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(search))
            {
                filter &= filterBuilder.Regex(j => j.Title, new MongoDB.Bson.BsonRegularExpression(search, "i"));
            }

            if (!string.IsNullOrEmpty(experienceLevel))
            {
                filter &= filterBuilder.Eq(j => j.ExperienceLevel, experienceLevel);
            }

            if (skills != null && skills.Count > 0)
            {
                filter &= filterBuilder.AnyIn(j => j.RequiredSkillTags, skills);
            }

            var total = await _context.Jobs.CountDocumentsAsync(filter, cancellationToken: cancellationToken);
            var jobs = await _context.Jobs.Find(filter)
                .SortByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync(cancellationToken);

            return (jobs, total);
        }

        public async Task<List<string>> GetDistinctLocationsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Jobs.Distinct<string>("location", Builders<Job>.Filter.Empty).ToListAsync(cancellationToken);
        }

        public async Task<List<string>> GetDistinctExperienceLevelsAsync(CancellationToken cancellationToken = default)
        {
            return await _context.Jobs.Distinct<string>("experience_level", Builders<Job>.Filter.Empty).ToListAsync(cancellationToken);
        }

        public async Task<List<string>> GetAllSkillTagsAsync(CancellationToken cancellationToken = default)
        {
            var allSkills = await _context.Jobs.Find(_ => true).Project(j => j.RequiredSkillTags).ToListAsync(cancellationToken);
            return allSkills.SelectMany(s => s).Distinct().ToList();
        }

        public async Task<List<Job>> GetByRecruiterIdAsync(string recruiterId, CancellationToken cancellationToken = default)
        {
            return await _context.Jobs.Find(j => j.RecruiterId == recruiterId)
                .SortByDescending(j => j.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<Job> GetByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _context.Jobs.Find(j => j.Id == id).FirstOrDefaultAsync(cancellationToken);
        }

        public async Task CreateAsync(Job job, CancellationToken cancellationToken = default)
        {
            await _context.Jobs.InsertOneAsync(job, cancellationToken: cancellationToken);
        }

        public async Task UpdateAsync(string id, Job job, CancellationToken cancellationToken = default)
        {
            await _context.Jobs.ReplaceOneAsync(j => j.Id == id, job, cancellationToken: cancellationToken);
        }

        public async Task DeleteAsync(string id, CancellationToken cancellationToken = default)
        {
            await _context.Jobs.DeleteOneAsync(j => j.Id == id, cancellationToken: cancellationToken);
        }
    }
}
