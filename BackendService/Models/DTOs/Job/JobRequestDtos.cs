namespace BackendService.Models.DTOs.Job
{
    public class CreateJobRequestDto
    {
        public string Title { get; set; } = null!;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public List<string>? Tags { get; set; }
        public string ExperienceLevel { get; set; } = string.Empty;
        public string? TargetRoadmapId { get; set; }
        public string? CompanyId { get; set; }
    }

    public class UpdateJobRequestDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public string? Salary { get; set; }
        public List<string>? Skills { get; set; }
        public List<string>? Tags { get; set; }
        public string? ExperienceLevel { get; set; }
        public string? TargetRoadmapId { get; set; }
    }
}
