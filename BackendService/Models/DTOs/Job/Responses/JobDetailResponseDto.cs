namespace BackendService.Models.DTOs.Job.Responses
{
    public class JobDetailResponseDto
    {
        public string CompanyId { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
        public string Salary { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = new();
        public string ExperienceLevel { get; set; } = string.Empty;
        public double MatchingRate { get; set; }
        public CompanyInfo Company { get; set; } = new();
    }
    public class CompanyInfo
    {
        public string Name { get; set; } = string.Empty;
        public string? LogoURL { get; set; }
        public string? WebsiteUrl { get; set; }

    }
}
