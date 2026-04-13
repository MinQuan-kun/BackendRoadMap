using BackendService.Data;
using BackendService.Models.DTOs.Job;
using BackendService.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
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

        [HttpGet("filters")]
        public async Task<ActionResult> GetJobFilters()
        {
            var skills = await _context.Jobs.Distinct<string>("skills", FilterDefinition<Job>.Empty).ToListAsync();
            var experienceLevels = await _context.Jobs.Distinct<string>("experience_level", FilterDefinition<Job>.Empty).ToListAsync();

            return Ok(new
            {
                skills = skills.Where(s => !string.IsNullOrWhiteSpace(s)).OrderBy(s => s).ToList(),
                experienceLevels = experienceLevels.Where(e => !string.IsNullOrWhiteSpace(e)).OrderBy(e => e).ToList()
            });
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

            var totalJobs = await _context.Jobs.CountDocumentsAsync(filter);

            var jobsQuery = await _context.Jobs.Find(filter).ToListAsync();

            var orderedJobs = sortBy == "salary"
                ? jobsQuery
                    .OrderByDescending(j => ParseMinimumSalary(j.Salary))
                    .ThenByDescending(j => j.CreatedAt)
                    .ToList()
                : jobsQuery
                    .OrderByDescending(j => j.CreatedAt)
                    .ToList();

            var jobs = orderedJobs
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToList();

            var companyIds = jobs
                .Select(j => j.CompanyId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct()
                .ToList();

            var companies = await _context.Companies
                .Find(c => c.Id != null && companyIds.Contains(c.Id))
                .ToListAsync();

            var companyLookup = companies
                .Where(c => c.Id != null)
                .ToDictionary(c => c.Id!, c => c);

            var response = new List<JobResponseDto>();
            foreach (var job in jobs)
            {
                companyLookup.TryGetValue(job.CompanyId, out var company);

                response.Add(new JobResponseDto
                {
                    Id = job.Id,
                    Title = job.Title,
                    CompanyName = company?.CompanyName ?? "Unknown",
                    CompanyLogo = company?.LogoUrl,
                    Location = job.Location,
                    Salary = job.Salary,
                    ExperienceLevel = job.ExperienceLevel,
                    Skills = job.Skills,
                    MatchingRate = job.MatchingRate,
                    PostedAt = GetRelativeTime(job.CreatedAt)
                });
            }

            return Ok(new { total = totalJobs, data = response, page, pageSize });
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<JobDetailResponseDto>> GetJobById(string id)
        {
            var job = await _context.Jobs.Find(j => j.Id == id).FirstOrDefaultAsync();
            if (job == null)
            {
                return NotFound("Không tìm thấy công việc.");
            }

            var company = await _context.Companies.Find(c => c.Id == job.CompanyId).FirstOrDefaultAsync();

            return Ok(new JobDetailResponseDto
            {
                Id = job.Id,
                Title = job.Title,
                Description = job.Description,
                CompanyId = job.CompanyId,
                CompanyName = company?.CompanyName ?? "Unknown",
                CompanyLogo = company?.LogoUrl,
                CompanyWebsite = company?.WebsiteUrl,
                Location = job.Location,
                Salary = job.Salary,
                Skills = job.Skills,
                ExperienceLevel = job.ExperienceLevel,
                TargetRoadmapId = job.TargetRoadmapId,
                MatchingRate = job.MatchingRate,
                PostedAt = GetRelativeTime(job.CreatedAt)
            });
        }

        [HttpGet("{jobId}/matching-score")]
        public async Task<ActionResult> GetMatchingScore(string jobId, [FromQuery] string? userId)
        {
            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var targetUserId = string.IsNullOrWhiteSpace(userId) ? userIdFromToken : userId;

            if (string.IsNullOrWhiteSpace(targetUserId))
            {
                return BadRequest("Thiếu userId để tính matching score.");
            }

            var job = await _context.Jobs.Find(j => j.Id == jobId).FirstOrDefaultAsync();
            if (job == null)
            {
                return NotFound("Không tìm thấy công việc.");
            }

            var user = await _context.Users.Find(u => u.Id == targetUserId).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            var matchingScore = await CalculateMatchingScoreAsync(user, job);

            return Ok(new
            {
                jobId,
                userId = targetUserId,
                matchingScore
            });
        }

        [Authorize]
        [HttpPost("{jobId}/apply")]
        public async Task<ActionResult<ApplyJobResponseDto>> ApplyForJob(string jobId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized("Không thể xác định người dùng hiện tại.");
            }

            var job = await _context.Jobs.Find(j => j.Id == jobId).FirstOrDefaultAsync();
            if (job == null)
            {
                return NotFound("Không tìm thấy công việc.");
            }

            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("Không tìm thấy người dùng.");
            }

            var existed = await _context.Applications
                .Find(a => a.JobId == jobId && a.UserId == userId)
                .AnyAsync();

            if (existed)
            {
                return Conflict("Bạn đã ứng tuyển công việc này.");
            }

            var matchingScore = await CalculateMatchingScoreAsync(user, job);

            var application = new Application
            {
                JobId = jobId,
                UserId = userId,
                MatchingScore = matchingScore,
                Status = "Pending",
                AppliedAt = DateTime.UtcNow
            };

            await _context.Applications.InsertOneAsync(application);

            return Ok(new ApplyJobResponseDto
            {
                ApplicationId = application.Id,
                JobId = application.JobId,
                UserId = application.UserId,
                MatchingScore = application.MatchingScore,
                Status = application.Status
            });
        }

        [HttpPost("seed-samples")]
        public async Task<ActionResult> SeedSampleJobs([FromQuery] int count = 10)
        {
            var sampleCount = Math.Clamp(count, 1, 50);

            var company = await _context.Companies.Find(FilterDefinition<Company>.Empty).FirstOrDefaultAsync();
            if (company == null)
            {
                company = new Company
                {
                    CompanyName = "GameNode Studio",
                    LogoUrl = null,
                    WebsiteUrl = "https://gamenode.example.com",
                    AdminIds = new List<string>()
                };

                await _context.Companies.InsertOneAsync(company);
            }

            var roadmap = await _context.Roadmaps.Find(FilterDefinition<Roadmap>.Empty).FirstOrDefaultAsync();
            if (roadmap == null)
            {
                var nodes = await _context.Nodes.Find(FilterDefinition<Node>.Empty).Limit(8).ToListAsync();

                roadmap = new Roadmap
                {
                    Title = "Sample Unity Career Roadmap",
                    Engine = "Unity",
                    Description = "Lộ trình mẫu phục vụ test tuyển dụng.",
                    CreatedAt = DateTime.UtcNow,
                    NodesLayout = nodes
                        .Where(n => n.Id != null)
                        .Select((n, index) => new NodeLayout
                        {
                            NodeId = n.Id!,
                            X = 80 + (index % 4) * 220,
                            Y = 120 + (index / 4) * 180
                        })
                        .ToList()
                };

                await _context.Roadmaps.InsertOneAsync(roadmap);
            }

            var now = DateTime.UtcNow;
            var titles = new[]
            {
                "Junior Unity Developer",
                "Gameplay Programmer (Unity)",
                "Unreal C++ Developer",
                "Technical Artist (Shader)",
                "Game Backend Integrator",
                "UI Programmer (Unity)",
                "AI Gameplay Engineer",
                "Mobile Game Developer",
                "Tools Developer (Game Pipeline)",
                "Mid-level Game Programmer"
            };

            var locations = new[]
            {
                "Hồ Chí Minh",
                "Hà Nội",
                "Đà Nẵng",
                "Remote"
            };

            var skillPools = new[]
            {
                new List<string> { "Unity", "C#", "OOP" },
                new List<string> { "Unity", "Shader", "VFX" },
                new List<string> { "Unreal", "C++", "Gameplay" },
                new List<string> { "C#", "AI", "NavMesh" },
                new List<string> { "Unity", "UI", "TextMeshPro" }
            };

            var expLevels = new[] { "Intern/Fresher", "1-3 năm", "3-5 năm" };

            var seedJobs = Enumerable.Range(0, sampleCount).Select(index => new Job
            {
                CompanyId = company.Id!,
                Title = titles[index % titles.Length],
                Description = $"Công việc mẫu #{index + 1} để test trang Jobs và quy trình ứng tuyển.",
                Location = locations[index % locations.Length],
                Salary = index % 3 == 0 ? "$800 - $1,200" : index % 3 == 1 ? "$1,200 - $2,000" : "$2,000 - $3,500",
                Skills = skillPools[index % skillPools.Length],
                ExperienceLevel = expLevels[index % expLevels.Length],
                TargetRoadmapId = roadmap.Id!,
                MatchingRate = 55 + (index % 6) * 7,
                CreatedAt = now.AddHours(-index * 5)
            }).ToList();

            await _context.Jobs.InsertManyAsync(seedJobs);

            return Ok(new
            {
                message = $"Đã tạo {sampleCount} jobs mẫu.",
                companyId = company.Id,
                roadmapId = roadmap.Id,
                createdJobs = seedJobs.Select(j => new { j.Id, j.Title, j.ExperienceLevel })
            });
        }

        private async Task<double> CalculateMatchingScoreAsync(User user, Job job)
        {
            if (string.IsNullOrWhiteSpace(job.TargetRoadmapId))
            {
                return 0;
            }

            var roadmap = await _context.Roadmaps.Find(r => r.Id == job.TargetRoadmapId).FirstOrDefaultAsync();
            if (roadmap == null || roadmap.NodesLayout.Count == 0)
            {
                return 0;
            }

            var targetNodeIds = roadmap.NodesLayout
                .Select(n => n.NodeId)
                .Where(id => !string.IsNullOrWhiteSpace(id))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToList();

            if (targetNodeIds.Count == 0)
            {
                return 0;
            }

            var completedNodeSet = new HashSet<string>(
                user.CompletedNodes.Where(id => !string.IsNullOrWhiteSpace(id)),
                StringComparer.OrdinalIgnoreCase);

            var matchedCount = targetNodeIds.Count(completedNodeSet.Contains);
            return Math.Round((double)matchedCount / targetNodeIds.Count * 100, 2);
        }

        private string GetRelativeTime(DateTime date)
        {
            var ts = DateTime.UtcNow - date;
            if (ts.TotalDays > 1) return $"{(int)ts.TotalDays} ngày trước";
            if (ts.TotalHours > 1) return $"{(int)ts.TotalHours} giờ trước";
            return "Vừa xong";
        }

        private static long ParseMinimumSalary(string? salary)
        {
            if (string.IsNullOrWhiteSpace(salary))
            {
                return 0;
            }

            var match = Regex.Match(salary, @"\d[\d,]*");
            if (!match.Success)
            {
                return 0;
            }

            var normalized = match.Value.Replace(",", string.Empty);
            return long.TryParse(normalized, out var value) ? value : 0;
        }
    }
}