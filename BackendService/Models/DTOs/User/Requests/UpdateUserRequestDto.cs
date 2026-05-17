using BackendService.Models.Entities;

namespace BackendService.Models.DTOs.User.Requests
{
    public class UpdateUserRequestDto
    {
        public string? UserName { get; set; }
        public string? Email { get; set; }
        public string? DisplayName { get; set; }
        public string? Bio { get; set; }
        public UserRole? Role { get; set; }
    }
}
