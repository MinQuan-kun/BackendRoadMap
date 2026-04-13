namespace BackendService.Models.DTOs.Job
{
    public class ApplyJobResponseDto
    {
        public string? ApplicationId { get; set; }
        public string JobId { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public double MatchingScore { get; set; }
        public string Status { get; set; } = null!;
    }
}