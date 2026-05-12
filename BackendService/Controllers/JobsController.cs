using BackendService.Data;
using BackendService.Models.DTOs.Job;
using BackendService.Models.Entities;
using Microsoft.AspNetCore.Authorization;
using BackendService.Services.Interface;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using System.Text.RegularExpressions;
using MongoDB.Driver;
using JobResponseDto = BackendService.Models.DTOs.Job.Responses.JobResponseDto;
using JobDetailResponseDto = BackendService.Models.DTOs.Job.JobDetailResponseDto;
using ApplyJobResponseDto = BackendService.Models.DTOs.Job.ApplyJobResponseDto;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/jobs")]
    public class JobsController(IJobService jobService, MongoDbContext context) : ControllerBase
    {
        private readonly MongoDbContext _context = context;
        private readonly IJobService _jobService = jobService;


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

        // ═══════════════════════════════════════════════════
        // ═══ RECRUITER ENDPOINTS ═══════════════════════════
        // ═══════════════════════════════════════════════════

        // Tạo bài tuyển dụng (chỉ recruiter đã được duyệt)
        [Authorize]
        [HttpPost]
        public async Task<ActionResult> CreateJob([FromBody] CreateJobRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return NotFound("Người dùng không tồn tại.");
            if (user.Role != 2) return Forbid();
            if (!user.IsApproved) return BadRequest(new { message = "Tài khoản nhà tuyển dụng chưa được phê duyệt bởi Admin." });

            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { message = "Tiêu đề công việc không được để trống." });
            }

            // Xử lý Company: dùng CompanyId từ request, hoặc tự tạo Company cho recruiter
            var companyId = request.CompanyId;
            if (string.IsNullOrWhiteSpace(companyId))
            {
                // Tìm company mà recruiter quản lý
                var existingCompany = await _context.Companies
                    .Find(c => c.AdminIds.Contains(userId))
                    .FirstOrDefaultAsync();

                if (existingCompany != null)
                {
                    companyId = existingCompany.Id;
                }
                else
                {
                    // Tạo company mới cho recruiter
                    var newCompany = new Company
                    {
                        CompanyName = user.FullName ?? user.UserName,
                        AdminIds = new List<string> { userId }
                    };
                    await _context.Companies.InsertOneAsync(newCompany);
                    companyId = newCompany.Id;
                }
            }

            var job = new Job
            {
                Title = request.Title.Trim(),
                Description = request.Description,
                Location = request.Location,
                Salary = request.Salary,
                Skills = request.Skills,
                Tags = request.Tags ?? new List<string>(),
                ExperienceLevel = request.ExperienceLevel,
                TargetRoadmapId = request.TargetRoadmapId ?? string.Empty,
                CompanyId = companyId!,
                CreatorId = userId,
                CreatedAt = DateTime.UtcNow
            };

            await _context.Jobs.InsertOneAsync(job);

            return CreatedAtAction(nameof(GetJobById), new { id = job.Id }, new
            {
                message = "Tạo bài tuyển dụng thành công.",
                job = new
                {
                    job.Id,
                    job.Title,
                    job.Location,
                    job.Salary,
                    job.Skills,
                    job.ExperienceLevel,
                    job.CreatorId,
                    job.CompanyId,
                    job.CreatedAt
                }
            });
        }

        // Cập nhật bài tuyển dụng
        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateJob(string id, [FromBody] UpdateJobRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var job = await _context.Jobs.Find(j => j.Id == id).FirstOrDefaultAsync();
            if (job == null) return NotFound("Không tìm thấy công việc.");

            // Chỉ cho phép creator hoặc admin sửa
            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return NotFound("Người dùng không tồn tại.");
            
            if (job.CreatorId != userId && user.Role != 0)
            {
                return Forbid();
            }

            if (request.Title != null) job.Title = request.Title.Trim();
            if (request.Description != null) job.Description = request.Description;
            if (request.Location != null) job.Location = request.Location;
            if (request.Salary != null) job.Salary = request.Salary;
            if (request.Skills != null) job.Skills = request.Skills;
            if (request.Tags != null) job.Tags = request.Tags;
            if (request.ExperienceLevel != null) job.ExperienceLevel = request.ExperienceLevel;
            if (request.TargetRoadmapId != null) job.TargetRoadmapId = request.TargetRoadmapId;

            await _context.Jobs.ReplaceOneAsync(j => j.Id == id, job);

            return Ok(new { message = "Cập nhật bài tuyển dụng thành công.", jobId = id });
        }

        // Xóa bài tuyển dụng
        [Authorize]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJob(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var job = await _context.Jobs.Find(j => j.Id == id).FirstOrDefaultAsync();
            if (job == null) return NotFound("Không tìm thấy công việc.");

            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null) return NotFound("Người dùng không tồn tại.");

            if (job.CreatorId != userId && user.Role != 0)
            {
                return Forbid();
            }

            // Xóa cả applications liên quan
            await _context.Applications.DeleteManyAsync(a => a.JobId == id);
            await _context.Jobs.DeleteOneAsync(j => j.Id == id);

            return Ok(new { message = "Xóa bài tuyển dụng thành công.", id });
        }

        // Tạo/Cập nhật roadmap riêng cho bài tuyển dụng
        [Authorize]
        [HttpPost("{id}/roadmap")]
        public async Task<IActionResult> CreateJobRoadmap(string id, [FromBody] BackendService.Models.DTOs.SaveRoadmapRequestDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var job = await _context.Jobs.Find(j => j.Id == id).FirstOrDefaultAsync();
            if (job == null) return NotFound("Công việc không tồn tại.");

            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (job.CreatorId != userId && user?.Role != 0) return Forbid();

            // Create or update roadmap
            var roadmapId = job.TargetRoadmapId;
            Roadmap? roadmap = null;

            if (!string.IsNullOrEmpty(roadmapId))
            {
                roadmap = await _context.Roadmaps.Find(r => r.Id == roadmapId).FirstOrDefaultAsync();
            }

            if (roadmap == null)
            {
                roadmap = new Roadmap
                {
                    Title = request.Title ?? $"Roadmap for {job.Title}",
                    Engine = request.Engine ?? "Custom",
                    Description = request.Description,
                    CreatorId = userId,
                    CreatedAt = DateTime.UtcNow,
                    UpdatedAt = DateTime.UtcNow
                };
            }
            else
            {
                roadmap.Title = request.Title ?? roadmap.Title;
                roadmap.Engine = request.Engine ?? roadmap.Engine;
                roadmap.Description = request.Description ?? roadmap.Description;
                roadmap.UpdatedAt = DateTime.UtcNow;
            }

            var persisted = await PersistBuilderNodesAsync(request);
            roadmap.NodesLayout = persisted.NodesLayout;

            if (string.IsNullOrEmpty(roadmap.Id))
            {
                await _context.Roadmaps.InsertOneAsync(roadmap);
                job.TargetRoadmapId = roadmap.Id!;
                await _context.Jobs.ReplaceOneAsync(j => j.Id == job.Id, job);
            }
            else
            {
                await _context.Roadmaps.ReplaceOneAsync(r => r.Id == roadmap.Id, roadmap);
            }

            return Ok(new { message = "Roadmap đã được lưu và gán cho công việc", roadmapId = roadmap.Id });
        }

        private async Task<(List<NodeLayout> NodesLayout, Dictionary<string, string> NodeIdMap)> PersistBuilderNodesAsync(BackendService.Models.DTOs.SaveRoadmapRequestDto request)
        {
            var nodeIdMap = new Dictionary<string, string>();
            var persistedNodes = new Dictionary<string, Node>();
            var nodesLayout = new List<NodeLayout>();

            foreach (var builderNode in request.Nodes)
            {
                Node? node = null;

                if (!string.IsNullOrWhiteSpace(builderNode.Id) && MongoDB.Bson.ObjectId.TryParse(builderNode.Id, out _))
                {
                    node = await _context.Nodes.Find(n => n.Id == builderNode.Id).FirstOrDefaultAsync();
                }

                if (node == null)
                {
                    node = new Node();
                }

                node.Name = string.IsNullOrWhiteSpace(builderNode.Content) ? "Untitled Node" : builderNode.Content;
                node.Engine = "Custom";
                node.Category = string.IsNullOrWhiteSpace(builderNode.Type) ? "default" : builderNode.Type;
                node.Description = builderNode.Link ?? string.Empty;
                node.Resources = new List<string>();

                if (string.IsNullOrWhiteSpace(node.Id))
                {
                    await _context.Nodes.InsertOneAsync(node);
                }
                else
                {
                    await _context.Nodes.ReplaceOneAsync(n => n.Id == node.Id, node);
                }

                nodeIdMap[builderNode.Id] = node.Id!;
                persistedNodes[node.Id!] = node;

                nodesLayout.Add(new NodeLayout
                {
                    NodeId = node.Id!,
                    X = builderNode.X,
                    Y = builderNode.Y,
                    Color = builderNode.Color,
                    Width = builderNode.Width > 0 ? builderNode.Width : null,
                    Height = builderNode.Height > 0 ? builderNode.Height : null,
                    Style = builderNode.Style
                });
            }

            var incomingParentMap = new Dictionary<string, string>();
            foreach (var connection in request.Connections)
            {
                if (!nodeIdMap.TryGetValue(connection.FromNodeId, out var sourceId) ||
                    !nodeIdMap.TryGetValue(connection.ToNodeId, out var targetId) ||
                    sourceId == targetId)
                {
                    continue;
                }

                if (!incomingParentMap.ContainsKey(targetId))
                {
                    incomingParentMap[targetId] = sourceId;
                }
            }

            foreach (var kvp in persistedNodes)
            {
                var nodeId = kvp.Key;
                var node = kvp.Value;

                if (incomingParentMap.TryGetValue(nodeId, out var parentId))
                {
                    node.ParentId = parentId;
                    node.Prerequisites = new List<string> { parentId };
                }
                else
                {
                    node.ParentId = null;
                    node.Prerequisites = new List<string>();
                }

                await _context.Nodes.ReplaceOneAsync(n => n.Id == nodeId, node);
            }

            return (nodesLayout, nodeIdMap);
        }

        [Authorize]
        [HttpGet("{id}/roadmap")]
        public async Task<IActionResult> GetJobRoadmap(string id)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var job = await _context.Jobs.Find(j => j.Id == id).FirstOrDefaultAsync();
            if (job == null) return NotFound("Công việc không tồn tại.");

            if (string.IsNullOrEmpty(job.TargetRoadmapId)) return Ok(null);

            // Fetch the roadmap from the database
            var roadmap = await _context.Roadmaps.Find(r => r.Id == job.TargetRoadmapId).FirstOrDefaultAsync();
            if (roadmap == null) return Ok(null);

            // Manually build the Response DTO reusing logic from RoadmapsController
            var nodeIds = roadmap.NodesLayout.Select(nl => nl.NodeId).ToList();
            var nodesData = await _context.Nodes.Find(n => n.Id != null && nodeIds.Contains(n.Id)).ToListAsync();
            var layoutLookup = roadmap.NodesLayout.ToDictionary(l => l.NodeId, l => l);

            var responseDto = new BackendService.Models.DTOs.RoadmapResponseDto
            {
                Id = roadmap.Id!,
                Title = roadmap.Title,
                Engine = roadmap.Engine,
                Description = roadmap.Description,
                CreatorId = roadmap.CreatorId,
                CreatedAt = roadmap.CreatedAt,
                UpdatedAt = roadmap.UpdatedAt,
                Nodes = nodesData.Select(n =>
                {
                    layoutLookup.TryGetValue(n.Id!, out var layout);
                    return new BackendService.Models.DTOs.FlowNodeDto
                    {
                        Id = n.Id!,
                        Type = string.IsNullOrWhiteSpace(n.Category) ? "default" : n.Category,
                        Position = new BackendService.Models.DTOs.FlowPosition
                        {
                            X = layout?.X ?? 0,
                            Y = layout?.Y ?? 0
                        },
                        Color = layout?.Color,
                        Style = layout?.Style,
                        Data = new BackendService.Models.DTOs.FlowData
                        {
                            Label = n.Name,
                            Description = n.Description,
                            Category = n.Category,
                            Resources = n.Resources,
                            Prerequisites = n.Prerequisites,
                            ContentBlocks = n.ContentBlocks,
                            VideoUrl = n.VideoUrl
                        }
                    };
                }).ToList(),
                Edges = nodesData.Where(n => n.ParentId != null && nodeIds.Contains(n.ParentId)).Select(n => new BackendService.Models.DTOs.FlowEdgeDto
                {
                    Id = $"e-{n.ParentId}-{n.Id}",
                    Source = n.ParentId!,
                    Target = n.Id!
                }).ToList()
            };

            return Ok(responseDto);
        }

        // Danh sách bài đăng của recruiter
        [Authorize]
        [HttpGet("my-posts")]
        public async Task<ActionResult> GetMyPosts()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var jobs = await _context.Jobs
                .Find(j => j.CreatorId == userId)
                .SortByDescending(j => j.CreatedAt)
                .ToListAsync();

            // Đếm số applications cho mỗi job
            var jobIds = jobs.Where(j => j.Id != null).Select(j => j.Id!).ToList();
            var allApplications = await _context.Applications
                .Find(a => jobIds.Contains(a.JobId))
                .ToListAsync();

            var applicantCountMap = allApplications
                .GroupBy(a => a.JobId)
                .ToDictionary(g => g.Key, g => g.Count());

            var companyIds = jobs.Select(j => j.CompanyId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            var companies = await _context.Companies
                .Find(c => c.Id != null && companyIds.Contains(c.Id))
                .ToListAsync();
            var companyLookup = companies.Where(c => c.Id != null).ToDictionary(c => c.Id!, c => c);

            var result = jobs.Select(job =>
            {
                companyLookup.TryGetValue(job.CompanyId, out var company);
                applicantCountMap.TryGetValue(job.Id!, out var applicantCount);

                return new
                {
                    id = job.Id,
                    title = job.Title,
                    description = job.Description,
                    location = job.Location,
                    salary = job.Salary,
                    skills = job.Skills,
                    experienceLevel = job.ExperienceLevel,
                    companyName = company?.CompanyName ?? "Unknown",
                    companyLogo = company?.LogoUrl,
                    applicantCount,
                    postedAt = GetRelativeTime(job.CreatedAt),
                    createdAt = job.CreatedAt
                };
            }).ToList();

            return Ok(new { total = result.Count, data = result });
        }

        // Danh sách ứng viên cho một job
        [Authorize]
        [HttpGet("{jobId}/applicants")]
        public async Task<ActionResult> GetJobApplicants(string jobId)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId))
            {
                return Unauthorized();
            }

            var job = await _context.Jobs.Find(j => j.Id == jobId).FirstOrDefaultAsync();
            if (job == null) return NotFound("Không tìm thấy công việc.");

            // Chỉ cho creator hoặc admin xem
            var currentUser = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (currentUser == null) return NotFound("Người dùng không tồn tại.");
            if (job.CreatorId != userId && currentUser.Role != 0)
            {
                return Forbid();
            }

            var applications = await _context.Applications
                .Find(a => a.JobId == jobId)
                .SortByDescending(a => a.MatchingScore)
                .ToListAsync();

            var applicantIds = applications.Select(a => a.UserId).Distinct().ToList();
            var applicants = await _context.Users
                .Find(u => u.Id != null && applicantIds.Contains(u.Id))
                .ToListAsync();
            var userLookup = applicants.Where(u => u.Id != null).ToDictionary(u => u.Id!, u => u);

            var result = applications.Select(app =>
            {
                userLookup.TryGetValue(app.UserId, out var applicant);
                return new
                {
                    applicationId = app.Id,
                    status = app.Status,
                    matchingScore = app.MatchingScore,
                    appliedAt = app.AppliedAt,
                    applicant = applicant != null ? new
                    {
                        id = applicant.Id,
                        userName = applicant.UserName,
                        fullName = applicant.FullName,
                        email = applicant.Email,
                        avatar = applicant.avatar,
                        skills = applicant.Skills,
                        completedNodes = applicant.CompletedNodes.Count
                    } : null
                };
            }).ToList();

            return Ok(new { jobId, total = result.Count, data = result });
        }

        // Cập nhật trạng thái ứng viên (Accepted / Rejected)
        [Authorize]
        [HttpPut("{jobId}/applicants/{applicationId}/status")]
        public async Task<ActionResult> UpdateApplicationStatus(string jobId, string applicationId, [FromBody] UpdateApplicationStatusDto request)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrWhiteSpace(userId)) return Unauthorized();

            var job = await _context.Jobs.Find(j => j.Id == jobId).FirstOrDefaultAsync();
            if (job == null) return NotFound("Không tìm thấy công việc.");

            var currentUser = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (currentUser == null) return NotFound();
            if (job.CreatorId != userId && currentUser.Role != 0) return Forbid();

            var application = await _context.Applications
                .Find(a => a.Id == applicationId && a.JobId == jobId)
                .FirstOrDefaultAsync();

            if (application == null) return NotFound("Không tìm thấy đơn ứng tuyển.");

            var validStatuses = new[] { "Pending", "Accepted", "Rejected", "Interview" };
            if (!validStatuses.Contains(request.Status))
            {
                return BadRequest(new { message = $"Trạng thái không hợp lệ. Cho phép: {string.Join(", ", validStatuses)}" });
            }

            var update = Builders<Application>.Update.Set(a => a.Status, request.Status);
            await _context.Applications.UpdateOneAsync(a => a.Id == applicationId, update);

            return Ok(new { message = "Cập nhật trạng thái thành công.", applicationId, status = request.Status });
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

    // DTO nhỏ cho cập nhật trạng thái
    public class UpdateApplicationStatusDto
    {
        public string Status { get; set; } = null!;
    }
}