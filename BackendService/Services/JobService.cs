using BackendService.Models.DTOs.Recruitment;
using BackendService.Models.Entities;
using BackendService.Models.Entities.Recruitment;
using BackendService.Repository.Interface;
using BackendService.Services.Interface;

namespace BackendService.Services
{
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IJobApplicationRepository _jobApplicationRepository;
        private readonly IUserRepository _userRepository;

        public JobService(IJobRepository jobRepository, IJobApplicationRepository jobApplicationRepository, IUserRepository userRepository)
        {
            _jobRepository = jobRepository;
            _jobApplicationRepository = jobApplicationRepository;
            _userRepository = userRepository;
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

        public async Task<(List<JobResponseDto> Jobs, long Total)> GetJobsPagedAsync(string? userId, string? search, string? experience, string? skills, int page, int pageSize, CancellationToken cancellationToken = default)
        {
            var skillList = !string.IsNullOrEmpty(skills) 
                ? skills.Split(',').Select(s => s.Trim()).ToList() 
                : null;

            var (jobs, total) = await _jobRepository.GetJobsAsync(search, experience, skillList, page, pageSize, cancellationToken);

            var userApplications = new List<string>();
            if (!string.IsNullOrEmpty(userId))
            {
                var applications = await _jobApplicationRepository.GetByUserIdAsync(userId, cancellationToken);
                userApplications = applications.Select(a => a.JobId).ToList();
            }

            var result = jobs.Select(j => new JobResponseDto
            {
                Id = j.Id,
                Title = j.Title,
                CompanyName = j.CompanyName,
                Location = j.Location,
                Salary = j.Salary ?? "Thỏa thuận",
                ExperienceLevel = j.ExperienceLevel,
                Skills = j.RequiredSkillTags,
                Description = j.Description,
                RoadmapGraphId = j.RoadmapGraphId,
                TargetRoadmapId = j.RoadmapGraphId,
                PostedAt = j.CreatedAt.ToString("dd/MM/yyyy"),
                MatchingRate = 0, // Placeholder
                HasApplied = userApplications.Contains(j.Id!)
            }).ToList();

            return (result, total);
        }

        public async Task<JobFiltersDto> GetFiltersAsync(CancellationToken cancellationToken = default)
        {
            var locations = await _jobRepository.GetDistinctLocationsAsync(cancellationToken);
            var levels = await _jobRepository.GetDistinctExperienceLevelsAsync(cancellationToken);
            var uniqueSkills = await _jobRepository.GetAllSkillTagsAsync(cancellationToken);

            return new JobFiltersDto
            {
                Locations = locations.Where(l => !string.IsNullOrEmpty(l)).ToList(),
                ExperienceLevels = levels.Where(l => !string.IsNullOrEmpty(l)).ToList(),
                Skills = uniqueSkills
            };
        }

        public async Task<Job> GetJobByIdAsync(string id, CancellationToken cancellationToken = default)
        {
            return await _jobRepository.GetByIdAsync(id, cancellationToken);
        }

        public async Task<Job> CreateJobAsync(string recruiterId, Job job, CancellationToken cancellationToken = default)
        {
            var recruiter = await _userRepository.GetByIdAsync(recruiterId, cancellationToken);
            if (recruiter == null) throw new KeyNotFoundException("Recruiter user not found.");

            job.Id = null;
            job.RecruiterId = recruiter.Id!;
            job.CompanyName = string.IsNullOrWhiteSpace(job.CompanyName)
                ? recruiter.DisplayName ?? recruiter.UserName
                : job.CompanyName;
            job.JobType = string.IsNullOrWhiteSpace(job.JobType) ? "remote" : job.JobType;
            job.CreatedAt = DateTime.UtcNow;

            await _jobRepository.CreateAsync(job, cancellationToken);
            return job;
        }

        public async Task<Job> UpdateJobAsync(string recruiterId, string id, Job job, CancellationToken cancellationToken = default)
        {
            var existingJob = await _jobRepository.GetByIdAsync(id, cancellationToken);
            if (existingJob == null) throw new KeyNotFoundException("Job not found.");
            if (existingJob.RecruiterId != recruiterId) throw new UnauthorizedAccessException("You do not own this job posting.");

            job.Id = existingJob.Id;
            job.RecruiterId = existingJob.RecruiterId;
            job.CreatedAt = existingJob.CreatedAt;
            job.CompanyName = string.IsNullOrWhiteSpace(job.CompanyName) ? existingJob.CompanyName : job.CompanyName;
            job.JobType = string.IsNullOrWhiteSpace(job.JobType) ? existingJob.JobType : job.JobType;
            job.RoadmapGraphId = string.IsNullOrWhiteSpace(job.RoadmapGraphId) ? existingJob.RoadmapGraphId : job.RoadmapGraphId;

            await _jobRepository.UpdateAsync(id, job, cancellationToken);
            return job;
        }

        public async Task DeleteJobAsync(string recruiterId, string id, CancellationToken cancellationToken = default)
        {
            var existingJob = await _jobRepository.GetByIdAsync(id, cancellationToken);
            if (existingJob == null) throw new KeyNotFoundException("Job not found.");
            if (existingJob.RecruiterId != recruiterId) throw new UnauthorizedAccessException("You do not own this job posting.");

            await _jobApplicationRepository.DeleteByJobIdAsync(id, cancellationToken);
            await _jobRepository.DeleteAsync(id, cancellationToken);
        }

        public async Task ApplyJobAsync(string userId, string jobId, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) throw new KeyNotFoundException("User not found.");

            if (user.Role == UserRole.Recruiter || user.Role == UserRole.Admin)
            {
                throw new InvalidOperationException("Tài khoản nhà tuyển dụng không thể ứng tuyển công việc.");
            }

            var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
            if (job == null) throw new KeyNotFoundException("Công việc không tồn tại.");

            if (job.RecruiterId == user.Id)
            {
                throw new InvalidOperationException("Bạn không thể ứng tuyển vào bài đăng của chính mình.");
            }

            var existing = await _jobApplicationRepository.GetByJobAndUserAsync(jobId, user.Id!, cancellationToken);
            if (existing != null) throw new InvalidOperationException("Bạn đã ứng tuyển công việc này rồi.");

            var application = new JobApplication
            {
                JobId = jobId,
                UserId = user.Id!,
                Status = "Pending",
                CreatedAt = DateTime.UtcNow
            };

            await _jobApplicationRepository.CreateAsync(application, cancellationToken);
        }

        public async Task<List<MyApplicationDto>> GetMyApplicationsAsync(string userId, CancellationToken cancellationToken = default)
        {
            var applications = await _jobApplicationRepository.GetByUserIdAsync(userId, cancellationToken);
            var jobIds = applications.Select(a => a.JobId).Distinct().ToList();

            var jobs = new List<Job>();
            foreach (var jobId in jobIds)
            {
                var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
                if (job != null) jobs.Add(job);
            }

            return applications.Select(a => {
                var job = jobs.FirstOrDefault(j => j.Id == a.JobId);
                return new MyApplicationDto
                {
                    ApplicationId = a.Id,
                    JobId = a.JobId,
                    Status = NormalizeStatus(a.Status),
                    AppliedAt = a.CreatedAt,
                    Job = job != null ? new JobShortDto
                    {
                        Title = job.Title,
                        Location = job.Location,
                        Salary = job.Salary
                    } : null,
                    Company = job != null ? new CompanyShortDto
                    {
                        Name = job.CompanyName,
                        Logo = ""
                    } : null
                };
            }).ToList();
        }

        public async Task<(List<MyJobPostDto> Posts, int Total)> GetMyPostsAsync(string recruiterId, CancellationToken cancellationToken = default)
        {
            var jobs = await _jobRepository.GetByRecruiterIdAsync(recruiterId, cancellationToken);

            var jobIds = jobs.Select(j => j.Id!).ToList();
            var applications = jobIds.Count == 0
                ? new List<JobApplication>()
                : await _jobApplicationRepository.GetByJobIdsAsync(jobIds, cancellationToken);

            var applicantCounts = applications
                .GroupBy(a => a.JobId)
                .ToDictionary(group => group.Key, group => group.Count());

            var data = jobs.Select(j => new MyJobPostDto
            {
                Id = j.Id,
                Title = j.Title,
                Description = j.Description,
                CompanyName = j.CompanyName,
                Location = j.Location,
                Salary = j.Salary,
                ExperienceLevel = j.ExperienceLevel,
                Skills = j.RequiredSkillTags,
                RequiredCourseIds = j.RequiredCourseIds,
                RoadmapGraphId = j.RoadmapGraphId,
                TargetRoadmapId = j.RoadmapGraphId,
                JobType = j.JobType,
                PostedAt = j.CreatedAt.ToString("dd/MM/yyyy"),
                CreatedAt = j.CreatedAt,
                ApplicantCount = applicantCounts.TryGetValue(j.Id!, out var count) ? count : 0
            }).ToList();

            return (data, jobs.Count);
        }

        public async Task<(List<ApplicantDto> Applicants, int Total)> GetApplicantsAsync(string recruiterId, string jobId, CancellationToken cancellationToken = default)
        {
            var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
            if (job == null) throw new KeyNotFoundException("Job not found.");
            if (job.RecruiterId != recruiterId) throw new UnauthorizedAccessException("You do not own this job posting.");

            var applications = await _jobApplicationRepository.GetByJobIdAsync(jobId, cancellationToken);
            var sortedApplications = applications.OrderByDescending(a => a.CreatedAt).ToList();

            var userIds = sortedApplications.Select(a => a.UserId).Distinct().ToList();
            var users = new List<User>();
            foreach (var userId in userIds)
            {
                var u = await _userRepository.GetByIdAsync(userId, cancellationToken);
                if (u != null) users.Add(u);
            }

            var data = sortedApplications.Select(application =>
            {
                var applicant = users.FirstOrDefault(u => u.Id == application.UserId);
                return new ApplicantDto
                {
                    ApplicationId = application.Id,
                    JobId = application.JobId,
                    Status = NormalizeStatus(application.Status),
                    Message = application.Message,
                    AppliedAt = application.CreatedAt,
                    Applicant = applicant == null ? null : new ApplicantDetailsDto
                    {
                        Id = applicant.Id,
                        FullName = applicant.DisplayName ?? applicant.UserName,
                        UserName = applicant.UserName,
                        Email = applicant.Email,
                        Avatar = applicant.AvatarUrl,
                        AvatarUrl = applicant.AvatarUrl,
                        Skills = applicant.SkillTags,
                        Role = applicant.Role.ToString(),
                        IsVerified = applicant.IsRecruiterVerified
                    }
                };
            }).ToList();

            return (data, sortedApplications.Count);
        }

        public async Task<JobApplication> UpdateApplicationStatusAsync(string recruiterId, string jobId, string applicationId, string status, CancellationToken cancellationToken = default)
        {
            var job = await _jobRepository.GetByIdAsync(jobId, cancellationToken);
            if (job == null) throw new KeyNotFoundException("Job not found.");
            if (job.RecruiterId != recruiterId) throw new UnauthorizedAccessException("You do not own this job posting.");

            var application = await _jobApplicationRepository.GetByIdAndJobIdAsync(applicationId, jobId, cancellationToken);
            if (application == null) throw new KeyNotFoundException("Đơn ứng tuyển không tồn tại.");

            application.Status = NormalizeStatus(status);
            await _jobApplicationRepository.UpdateAsync(applicationId, application, cancellationToken);

            return application;
        }
    }
}
