using Microsoft.AspNetCore.Mvc;
using BackendService.Models.Entities.Recruitment;
using BackendService.Services.Interface;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using BackendService.Models.DTOs.Recruitment;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        private string? GetCurrentUserId()
        {
            return User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        }

        // GET: api/jobs
        [HttpGet]
        public async Task<ActionResult> GetJobs([FromQuery] string? search, [FromQuery] string? experience, [FromQuery] string? skills, [FromQuery] int page = 1, [FromQuery] int pageSize = 5, CancellationToken cancellationToken = default)
        {
            var userId = GetCurrentUserId();
            var (jobs, total) = await _jobService.GetJobsPagedAsync(userId, search, experience, skills, page, pageSize, cancellationToken);
            return Ok(new { data = jobs, total = total });
        }

        // GET: api/jobs/filters
        [HttpGet("filters")]
        public async Task<ActionResult> GetFilters(CancellationToken cancellationToken)
        {
            var filters = await _jobService.GetFiltersAsync(cancellationToken);
            return Ok(filters);
        }

        // GET: api/jobs/{id}
        [HttpGet("{id}")]
        public async Task<ActionResult<Job>> GetJob(string id, CancellationToken cancellationToken)
        {
            var job = await _jobService.GetJobByIdAsync(id, cancellationToken);
            if (job == null) return NotFound();
            return Ok(job);
        }

        // POST: api/jobs (Recruiter only)
        [Authorize(Roles = "Recruiter")]
        [HttpPost]
        public async Task<ActionResult<Job>> CreateJob(Job job, CancellationToken cancellationToken)
        {
            var recruiterId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(recruiterId)) return Unauthorized();

            try
            {
                var createdJob = await _jobService.CreateJobAsync(recruiterId, job, cancellationToken);
                return CreatedAtAction(nameof(GetJob), new { id = createdJob.Id }, createdJob);
            }
            catch (KeyNotFoundException)
            {
                return Unauthorized();
            }
        }

        [Authorize(Roles = "Recruiter")]
        [HttpPut("{id}")]
        public async Task<ActionResult<Job>> UpdateJob(string id, Job job, CancellationToken cancellationToken)
        {
            var recruiterId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(recruiterId)) return Unauthorized();

            try
            {
                var updatedJob = await _jobService.UpdateJobAsync(recruiterId, id, job, cancellationToken);
                return Ok(updatedJob);
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [Authorize(Roles = "Recruiter")]
        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteJob(string id, CancellationToken cancellationToken)
        {
            var recruiterId = GetCurrentUserId();
            if (string.IsNullOrWhiteSpace(recruiterId)) return Unauthorized();

            try
            {
                await _jobService.DeleteJobAsync(recruiterId, id, cancellationToken);
                return Ok(new { message = "Đã xóa công việc." });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        // POST: api/jobs/{id}/apply
        [Authorize]
        [HttpPost("{id}/apply")]
        public async Task<ActionResult> ApplyJob(string id, CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                await _jobService.ApplyJobAsync(userId, id, cancellationToken);
                return Ok(new { message = "Ứng tuyển thành công!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        // GET: api/jobs/my-applications
        [Authorize]
        [HttpGet("my-applications")]
        public async Task<ActionResult> GetMyApplications(CancellationToken cancellationToken)
        {
            var userId = GetCurrentUserId();
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var applications = await _jobService.GetMyApplicationsAsync(userId, cancellationToken);
            return Ok(applications);
        }

        // GET: api/jobs/my-posts
        [Authorize(Roles = "Recruiter")]
        [HttpGet("my-posts")]
        public async Task<ActionResult> GetMyPosts(CancellationToken cancellationToken)
        {
            var recruiterId = GetCurrentUserId();
            if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

            var (posts, total) = await _jobService.GetMyPostsAsync(recruiterId, cancellationToken);
            return Ok(new { data = posts, total = total });
        }

        // GET: api/jobs/{id}/applicants
        [Authorize(Roles = "Recruiter")]
        [HttpGet("{id}/applicants")]
        public async Task<ActionResult> GetApplicants(string id, CancellationToken cancellationToken)
        {
            var recruiterId = GetCurrentUserId();
            if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

            try
            {
                var (applicants, total) = await _jobService.GetApplicantsAsync(recruiterId, id, cancellationToken);
                return Ok(new { data = applicants, total = total });
            }
            catch (KeyNotFoundException)
            {
                return NotFound();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        public class UpdateApplicationStatusRequest
        {
            public string Status { get; set; } = string.Empty;
        }

        // PUT: api/jobs/{jobId}/applicants/{applicationId}/status
        [Authorize(Roles = "Recruiter")]
        [HttpPut("{jobId}/applicants/{applicationId}/status")]
        public async Task<ActionResult> UpdateApplicationStatus(string jobId, string applicationId, [FromBody] UpdateApplicationStatusRequest request, CancellationToken cancellationToken)
        {
            var recruiterId = GetCurrentUserId();
            if (string.IsNullOrEmpty(recruiterId)) return Unauthorized();

            try
            {
                var application = await _jobService.UpdateApplicationStatusAsync(recruiterId, jobId, applicationId, request.Status, cancellationToken);
                return Ok(new
                {
                    message = "Đã cập nhật trạng thái đơn ứng tuyển.",
                    applicationId = application.Id,
                    status = application.Status
                });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(new { message = ex.Message });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
