using BackendService.Models.Entities;

namespace BackendService.Models.DTOs.User.Responses
{
    public class ResponseUserByIdDto
    {
        public string? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public UserRole Role { get; set; }
        public string? BirthDate { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public UserLinks Links { get; set; } = new();
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public List<string> Skills { get; set; } = new();
        public bool HasCompletedQuiz { get; set; }
        public List<string> CompletedNodes { get; set; } = new();
        public List<string> SkippedNodes { get; set; } = new();
    }
}
