using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.Entities;

namespace BackendService.Mapping
{
    public static class RegisterRequestDtoToUser
    {
        public static User Transform(RegisterRequestDto dto)
        {
            return new User
            {
                UserName = dto.UserName,
                PasswordHash = dto.Password,
                Email = dto.Email,
                Role = UserRole.User,
                Bio = string.Empty,
                CreatedAt = DateTime.UtcNow,
            };
        }
    }
}
