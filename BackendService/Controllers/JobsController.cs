using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BackendService.Data;
using BackendService.Models.Entities.Recruitment;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

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

        // GET: api/jobs
        [HttpGet]
        public async Task<ActionResult> GetJobs([FromQuery] string? search, [FromQuery] string? experience, [FromQuery] string? skills, [FromQuery] int page = 1, [FromQuery] int pageSize = 5)
        {
            var filterBuilder = Builders<Job>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(search))
            {
                filter &= filterBuilder.Regex(j => j.Title, new MongoDB.Bson.BsonRegularExpression(search, "i"));
            }

            if (!string.IsNullOrEmpty(experience))
            {
                filter &= filterBuilder.Eq(j => j.ExperienceLevel, experience);
            }

            if (!string.IsNullOrEmpty(skills))
            {
                var skillList = skills.Split(',').ToList();
                filter &= filterBuilder.AnyIn(j => j.RequiredSkillTags, skillList);
            }

            var total = await _context.Jobs.CountDocumentsAsync(filter);
            var jobs = await _context.Jobs.Find(filter)
                .SortByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Limit(pageSize)
                .ToListAsync();

            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            var userApplications = new List<string>();
            if (!string.IsNullOrEmpty(userId))
            {
                userApplications = await _context.JobApplications.Find(a => a.UserId == userId).Project(a => a.JobId).ToListAsync();
            }

            // Map to frontend structure
            var result = jobs.Select(j => new {
                id = j.Id,
                title = j.Title,
                companyName = j.CompanyName,
                location = j.Location,
                salary = j.Salary ?? "Thỏa thuận",
                experienceLevel = j.ExperienceLevel,
                skills = j.RequiredSkillTags,
                postedAt = j.CreatedAt.ToString("dd/MM/yyyy"),
                matchingRate = 0, // Placeholder
                hasApplied = userApplications.Contains(j.Id!)
            });

            return Ok(new { data = result, total = total });
        }

        // GET: api/jobs/filters
        [HttpGet("filters")]
        public async Task<ActionResult> GetFilters()
        {
            var locations = await _context.Jobs.Distinct<string>("location", Builders<Job>.Filter.Empty).ToListAsync();
            var levels = await _context.Jobs.Distinct<string>("experience_level", Builders<Job>.Filter.Empty).ToListAsync();
            var allSkills = await _context.Jobs.Find(_ => true).Project(j => j.RequiredSkillTags).ToListAsync();
            var uniqueSkills = allSkills.SelectMany(s => s).Distinct().ToList();
            
            return Ok(new { 
                locations = locations.Where(l => !string.IsNullOrEmpty(l)).ToList(), 
                experienceLevels = levels.Where(l => !string.IsNullOrEmpty(l)).ToList(),
                skills = uniqueSkills
            });
        }

        // GET: api/jobs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJob(string id)
        {
            var job = await _context.Jobs.Find(j => j.Id == id).FirstOrDefaultAsync();
            if (job == null) return NotFound();
            return Ok(job);
        }

        // POST: api/jobs (Recruiter only)
        [HttpPost]
        public async Task<ActionResult<Job>> CreateJob(Job job)
        {
            job.CreatedAt = DateTime.UtcNow;
            await _context.Jobs.InsertOneAsync(job);
            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
        }

        // POST: api/jobs/{id}/apply
        [Authorize]
        [HttpPost("{id}/apply")]
        public async Task<ActionResult> ApplyJob(string id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // Check if already applied
            var existing = await _context.JobApplications.Find(a => a.JobId == id && a.UserId == userId).FirstOrDefaultAsync();
            if (existing != null) return BadRequest(new { message = "Bạn đã ứng tuyển công việc này rồi." });

            var application = new JobApplication
            {
                JobId = id,
                UserId = userId,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _context.JobApplications.InsertOneAsync(application);
            return Ok(new { message = "Ứng tuyển thành công!" });
        }

        // GET: api/jobs/my-applications
        [Authorize]
        [HttpGet("my-applications")]
        public async Task<ActionResult> GetMyApplications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var applications = await _context.JobApplications.Find(a => a.UserId == userId).ToListAsync();
            
            var jobIds = applications.Select(a => a.JobId).Distinct().ToList();
            var jobs = await _context.Jobs.Find(j => jobIds.Contains(j.Id!)).ToListAsync();

            var result = applications.Select(a => {
                var job = jobs.FirstOrDefault(j => j.Id == a.JobId);
                return new {
                    applicationId = a.Id,
                    jobId = a.JobId,
                    status = a.Status,
                    appliedAt = a.CreatedAt,
                    job = job != null ? new {
                        title = job.Title,
                        location = job.Location,
                        salary = job.Salary,
                    } : null,
                    company = job != null ? new {
                        name = job.CompanyName,
                        logo = "" // Placeholder
                    } : null
                };
            });

            return Ok(result);
        }

        // GET: api/jobs/my-posts
        [HttpGet("my-posts")]
        public async Task<ActionResult> GetMyPosts()
        {
            // Returns all jobs for now, should filter by RecruiterId in real scenario
            var jobs = await _context.Jobs.Find(_ => true).ToListAsync();
            return Ok(new { data = jobs, total = jobs.Count });
        }

        // GET: api/jobs/{id}/applicants
        [HttpGet("{id}/applicants")]
        public async Task<ActionResult> GetApplicants(string id)
        {
            var applicants = await _context.JobApplications.Find(a => a.JobId == id).ToListAsync();
            return Ok(applicants);
        }
    }
}
