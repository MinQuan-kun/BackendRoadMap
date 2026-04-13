using BackendService.Models.DTOs.User.Responses;
using BackendService.Models.Entities;

namespace BackendService.Mapping
{
    public static class UserToRegisterResponseDto
    {
        public static RegisterResponseDto Transform(User user)
        {
            return new RegisterResponseDto
            {
                Id = user.Id, 
                UserName = user.UserName,
                Role = user.Role
            };
        }
    }
}
