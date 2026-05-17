using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BackendService.Data;
using BackendService.Models.Entities.Recruitment;
using BackendService.Models.Entities;
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

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        private static string NormalizeStatus(string? status)
        {
            if (string.IsNullOrWhiteSpace(status)) return "Pending";

            return status.Trim().ToLowerInvariant() switch
            {
                "pending" => "Pending",
                "interview" => "Interview",
                "accepted" => "Accepted",
                "rejected" => "Rejected",
                _ => char.ToUpperInvariant(status.Trim()[0]) + status.Trim()[1..].ToLowerInvariant(),
            };
        }

        private async Task<User?> GetCurrentUserAsync()
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(userId)) return null;

            return await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
        }

        private async Task<Job?> GetOwnedJobAsync(string jobId, string recruiterId)
        {
            return await _context.Jobs
                .Find(j => j.Id == jobId && j.RecruiterId == recruiterId)
                .FirstOrDefaultAsync();
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
                description = j.Description,
                roadmapGraphId = j.RoadmapGraphId,
                targetRoadmapId = j.RoadmapGraphId,
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
        [Authorize(Roles = "Recruiter")]
        [HttpPost]
        public async Task<ActionResult<Job>> CreateJob(Job job)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Unauthorized();

            job.Id = null;
            job.RecruiterId = currentUser.Id!;
            job.CompanyName = string.IsNullOrWhiteSpace(job.CompanyName)
                ? currentUser.DisplayName ?? currentUser.UserName
                : job.CompanyName;
            job.JobType = string.IsNullOrWhiteSpace(job.JobType) ? "remote" : job.JobType;
            job.CreatedAt = DateTime.UtcNow;
            await _context.Jobs.InsertOneAsync(job);
            return CreatedAtAction(nameof(GetJob), new { id = job.Id }, job);
        }

        [Authorize(Roles = "Recruiter")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Job>> UpdateJob(string id, Job job)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Unauthorized();

            var existingJob = await GetOwnedJobAsync(id, currentUser.Id!);
            if (existingJob == null) return Forbid();

            job.Id = existingJob.Id;
            job.RecruiterId = existingJob.RecruiterId;
            job.CreatedAt = existingJob.CreatedAt;
            job.CompanyName = string.IsNullOrWhiteSpace(job.CompanyName)
                ? existingJob.CompanyName
                : job.CompanyName;
            job.JobType = string.IsNullOrWhiteSpace(job.JobType)
                ? existingJob.JobType
                : job.JobType;
            job.RoadmapGraphId = string.IsNullOrWhiteSpace(job.RoadmapGraphId)
                ? existingJob.RoadmapGraphId
                : job.RoadmapGraphId;

            await _context.Jobs.ReplaceOneAsync(j => j.Id == id, job);
            return Ok(job);
        }

        [Authorize(Roles = "Recruiter")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJob(string id)
        {
            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Unauthorized();

            var existingJob = await GetOwnedJobAsync(id, currentUser.Id!);
            if (existingJob == null) return Forbid();

            await _context.JobApplications.DeleteManyAsync(a => a.JobId == id);
            await _context.Jobs.DeleteOneAsync(j => j.Id == id);

            return Ok(new { message = "Đã xóa công việc." });
        }

        // POST: api/jobs/{id}/apply
        [Authorize]
        [HttpPost("{id}/apply")]
        public async Task<ActionResult> ApplyJob(string id)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var currentUser = await GetCurrentUserAsync();
            if (currentUser == null) return Unauthorized();

            if (currentUser.Role == UserRole.Recruiter || currentUser.Role == UserRole.Admin)
            {
                return BadRequest(new { message = "Tài khoản nhà tuyển dụng không thể ứng tuyển công việc." });
            }

            var job = await _context.Jobs.Find(j => j.Id == id).FirstOrDefaultAsync();
            if (job == null) return NotFound(new { message = "Công việc không tồn tại." });

            if (job.RecruiterId == userId)
            {
                return BadRequest(new { message = "Bạn không thể ứng tuyển vào bài đăng của chính mình." });
            }

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
                    status = NormalizeStatus(a.Status),
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
        [Authorize(Roles = "Recruiter")]
        [HttpGet("my-posts")]
        public async Task<ActionResult> GetMyPosts()
        {
            var recruiterId = GetCurrentUserId();
            if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

            var jobs = await _context.Jobs.Find(j => j.RecruiterId == recruiterId)
                .SortByDescending(j => j.CreatedAt)
                .ToListAsync();

            var jobIds = jobs.Select(j => j.Id!).ToList();
            var applications = jobIds.Count == 0
                ? new List<JobApplication>()
                : await _context.JobApplications.Find(a => jobIds.Contains(a.JobId)).ToListAsync();

            var applicantCounts = applications
                .GroupBy(a => a.JobId)
                .ToDictionary(group => group.Key, group => group.Count());

            var data = jobs.Select(j => new
            {
                id = j.Id,
                title = j.Title,
                description = j.Description,
                companyName = j.CompanyName,
                location = j.Location,
                salary = j.Salary,
                experienceLevel = j.ExperienceLevel,
                skills = j.RequiredSkillTags,
                requiredCourseIds = j.RequiredCourseIds,
                roadmapGraphId = j.RoadmapGraphId,
                targetRoadmapId = j.RoadmapGraphId,
                jobType = j.JobType,
                postedAt = j.CreatedAt.ToString("dd/MM/yyyy"),
                createdAt = j.CreatedAt,
                applicantCount = applicantCounts.TryGetValue(j.Id!, out var count) ? count : 0
            });

            return Ok(new { data, total = jobs.Count });
        }

        // GET: api/jobs/{id}/applicants
        [Authorize(Roles = "Recruiter")]
        [HttpGet("{id}/applicants")]
        public async Task<ActionResult> GetApplicants(string id)
        {
            var recruiterId = GetCurrentUserId();
            if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

            var job = await GetOwnedJobAsync(id, recruiterId);
            if (job == null) return Forbid();

            var applications = await _context.JobApplications
                .Find(a => a.JobId == id)
                .SortByDescending(a => a.CreatedAt)
                .ToListAsync();

            var userIds = applications.Select(a => a.UserId).Distinct().ToList();
            var users = userIds.Count == 0
                ? new List<User>()
                : await _context.Users.Find(u => userIds.Contains(u.Id!)).ToListAsync();

            var data = applications.Select(application =>
            {
                var applicant = users.FirstOrDefault(u => u.Id == application.UserId);
                return new
                {
                    applicationId = application.Id,
                    jobId = application.JobId,
                    status = NormalizeStatus(application.Status),
                    message = application.Message,
                    appliedAt = application.CreatedAt,
                    applicant = applicant == null ? null : new
                    {
                        id = applicant.Id,
                        fullName = applicant.DisplayName ?? applicant.UserName,
                        userName = applicant.UserName,
                        email = applicant.Email,
                        avatar = applicant.AvatarUrl,
                        avatarUrl = applicant.AvatarUrl,
                        skills = applicant.SkillTags,
                        role = applicant.Role.ToString(),
                        isVerified = applicant.IsRecruiterVerified
                    }
                };
            });

            return Ok(new { data, total = applications.Count });
        }

        public class UpdateApplicationStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        // PUT: api/jobs/{jobId}/applicants/{applicationId}/status
        [Authorize(Roles = "Recruiter")]
        [HttpPut("{jobId}/applicants/{applicationId}/status")]
        public async Task<ActionResult> UpdateApplicationStatus(string jobId, string applicationId, [FromBody] UpdateApplicationStatusRequest request)
        {
            var recruiterId = GetCurrentUserId();
            if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

            var job = await GetOwnedJobAsync(jobId, recruiterId);
            if (job == null) return Forbid();

            var application = await _context.JobApplications
                .Find(a => a.Id == applicationId && a.JobId == jobId)
                .FirstOrDefaultAsync();

            if (application == null) return NotFound(new { message = "Đơn ứng tuyển không tồn tại." });

            application.Status = NormalizeStatus(request.Status);
            await _context.JobApplications.ReplaceOneAsync(a => a.Id == applicationId, application);

            return Ok(new
            {
                message = "Đã cập nhật trạng thái đơn ứng tuyển.",
                applicationId = application.Id,
                status = application.Status
            });
        }
    }
}
