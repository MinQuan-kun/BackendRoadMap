using BackendService.Models.Entities;

namespace BackendService.Models.DTOs.User.Requests
{
    public class RegisterRequestDto
    {
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public UserRole Role { get; set; } = UserRole.User;
    }
}