namespace BackendService.Models.DTOs.Job.Responses
{
    public class JobResponseDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string? CompanyLogo { get; set; }
        public string Location { get; set; } = null!;
        public string Salary { get; set; } = null!;
        public List<string> Skills { get; set; } = new();
        public double MatchingRate { get; set; }
        public string PostedAt { get; set; } = null!;
    }
}