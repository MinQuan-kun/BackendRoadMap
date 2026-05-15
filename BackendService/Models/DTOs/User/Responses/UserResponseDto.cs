using BackendService.Models.Entities;

namespace BackendService.Models.DTOs.User.Responses
{
    public class UserResponseDto
    {
        public string? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? DisplayName { get; set; }
        public string Email { get; set; } = string.Empty;
        public UserRole Role { get; set; }
        public UserStatus Status { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public DateTime? BirthDate { get; set; }
        public UserLinksDto? Links { get; set; }
        public List<string> SkillTags { get; set; } = new();
        public bool IsRecruiterVerified { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class UserLinksDto
    {
        public string? Github { get; set; }
        public string? Portfolio { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }
    }
}
