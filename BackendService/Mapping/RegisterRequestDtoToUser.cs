using BackendService.Models.DTOs.User;
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
                Password = dto.Password,
                Email = dto.Email,
                Role = 1, 
                bio = string.Empty, 
                CreateAt = DateTime.UtcNow, 
            };
        }
    }
}
