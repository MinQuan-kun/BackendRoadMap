using BackendService.Models.DTOs.User.Responses;
using BackendService.Models.Entities;

namespace BackendService.Mapping
{
    public static class UserToUserResponseDto
    {
        public static UserResponseDto Transform(User user)
        {
            return new UserResponseDto
            {
                Id = user.Id,
                UserName = user.UserName,
                FullName = user.FullName,
                Email = user.Email,
                Role = user.Role,
                CompletedNodes = user.CompletedNodes,
                OnboardingResponses = user.OnboardingResponses
            };
        }
    }
}
