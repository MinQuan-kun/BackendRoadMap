using BackendService.Data;
using BackendService.Models.DTOs.Job;
using BackendService.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public JobsController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult> GetJobs(
            [FromQuery] string? search,
            [FromQuery] string? skills,
            [FromQuery] string? experience,
            [FromQuery] string sortBy = "newest",
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 10)
        {
            var filterBuilder = Builders<Job>.Filter;
            var filter = filterBuilder.Empty;

            // Lọc theo từ khóa tìm kiếm (Title hoặc Description)
            if (!string.IsNullOrEmpty(search))
            {
                filter &= filterBuilder.Regex(j => j.Title, new MongoDB.Bson.BsonRegularExpression(search, "i"));
            }

            // Lọc theo danh sách kỹ năng
            if (!string.IsNullOrEmpty(skills))
            {
                var skillList = skills.Split(',').ToList();
                filter &= filterBuilder.AnyIn(j => j.Skills, skillList);
            }

            // Lọc theo cấp độ kinh nghiệm
            if (!string.IsNullOrEmpty(experience))
            {
                filter &= filterBuilder.Eq(j => j.ExperienceLevel, experience);
            }

            // Sắp xếp
            var sort = sortBy == "salary"
                ? Builders<Job>.Sort.Descending(j => j.Salary)
                : Builders<Job>.Sort.Descending(j => j.CreatedAt);

            var totalJobs = await _context.Jobs.CountDocumentsAsync(filter);

            var jobs = await _context.Jobs.Find(filter)
                .Sort(sort)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            // Kết hợp với thông tin Công ty
            var response = new List<JobResponseDto>();
            foreach (var job in jobs)
            {
                var company = await _context.Companies.Find(c => c.Id == job.CompanyId).FirstOrDefaultAsync();
                response.Add(new JobResponseDto
                {
                    Id = job.Id,
                    Title = job.Title,
                    CompanyName = company?.CompanyName ?? "Unknown",
                    CompanyLogo = company?.LogoUrl,
                    Location = job.Location,
                    Salary = job.Salary,
                    Skills = job.Skills,
                    MatchingRate = job.MatchingRate,
                    PostedAt = GetRelativeTime(job.CreatedAt)
                });
            }

            return Ok(new { total = totalJobs, data = response, page, pageSize });
        }

        private string GetRelativeTime(DateTime date)
        {
            var ts = DateTime.UtcNow - date;
            if (ts.TotalDays > 1) return $"{(int)ts.TotalDays} ngày trước";
            if (ts.TotalHours > 1) return $"{(int)ts.TotalHours} giờ trước";
            return "Vừa xong";
        }
    }
}