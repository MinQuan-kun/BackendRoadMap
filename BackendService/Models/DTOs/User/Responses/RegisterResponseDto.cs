using BackendService.Models.Entities;

namespace BackendService.Models.DTOs.User.Responses
{
    public class RegisterResponseDto
    {
        public string? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public UserRole Role { get; set; }
    }
}
