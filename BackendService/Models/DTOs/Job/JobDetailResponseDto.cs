namespace BackendService.Models.DTOs.Job
{
    public class JobDetailResponseDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string CompanyId { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string? CompanyLogo { get; set; }
        public string? CompanyWebsite { get; set; }
        public string Location { get; set; } = null!;
        public string Salary { get; set; } = null!;
        public List<string> Skills { get; set; } = new();
        public string ExperienceLevel { get; set; } = null!;
        public string TargetRoadmapId { get; set; } = null!;
        public double MatchingRate { get; set; }
        public string PostedAt { get; set; } = null!;
    }
}