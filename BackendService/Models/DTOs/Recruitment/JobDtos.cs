using System;
using System.Collections.Generic;

namespace BackendService.Models.DTOs.Recruitment
{
    public class JobResponseDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string CompanyName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string Salary { get; set; } = "Thỏa thuận";
        public string? ExperienceLevel { get; set; }
        public List<string> Skills { get; set; } = new();
        public string? Description { get; set; }
        public string? RoadmapGraphId { get; set; }
        public string? TargetRoadmapId { get; set; }
        public string PostedAt { get; set; } = string.Empty;
        public int MatchingRate { get; set; }
        public bool HasApplied { get; set; }
    }

    public class JobFiltersDto
    {
        public List<string> Locations { get; set; } = new();
        public List<string> ExperienceLevels { get; set; } = new();
        public List<string> Skills { get; set; } = new();
    }

    public class MyApplicationDto
    {
        public string? ApplicationId { get; set; }
        public string JobId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public DateTime AppliedAt { get; set; }
        public JobShortDto? Job { get; set; }
        public CompanyShortDto? Company { get; set; }
    }

    public class JobShortDto
    {
        public string Title { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Salary { get; set; }
    }

    public class CompanyShortDto
    {
        public string Name { get; set; } = string.Empty;
        public string Logo { get; set; } = string.Empty;
    }

    public class MyJobPostDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Location { get; set; }
        public string? Salary { get; set; }
        public string? ExperienceLevel { get; set; }
        public List<string> Skills { get; set; } = new();
        public List<string> RequiredCourseIds { get; set; } = new();
        public string? RoadmapGraphId { get; set; }
        public string? TargetRoadmapId { get; set; }
        public string JobType { get; set; } = string.Empty;
        public string PostedAt { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
        public int ApplicantCount { get; set; }
    }

    public class ApplicantDto
    {
        public string? ApplicationId { get; set; }
        public string JobId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? Message { get; set; }
        public DateTime AppliedAt { get; set; }
        public ApplicantDetailsDto? Applicant { get; set; }
    }

    public class ApplicantDetailsDto
    {
        public string? Id { get; set; }
        public string FullName { get; set; } = string.Empty;
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Avatar { get; set; }
        public string? AvatarUrl { get; set; }
        public List<string> Skills { get; set; } = new();
        public string Role { get; set; } = string.Empty;
        public bool IsVerified { get; set; }
    }
}
