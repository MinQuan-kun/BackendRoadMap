using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.DTOs.Application.Responses
{
    public class ApplicationResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public ListUserForApplicationResponseDto User { get; set; } = new();
        public ListJobForApplicationResponseDto Job { get; set; } = new();
        public double MatchingScore { get; set; }
        public string Status { get; set; } = "Pending"; 

    }
    public class ListUserForApplicationResponseDto
    {
        public string Email { get; set; } = string.Empty;
        public string Username { get; set; } = string.Empty;
    }
    public class ListJobForApplicationResponseDto 
    {
        public string CompanyId { get; set; } = string.Empty;
        public CompanyForJobResponseDto Company { get; set; } = new();
    }

    public class  CompanyForJobResponseDto
    {
        public string CompanyName { get; set; } = string.Empty;
    }
}
