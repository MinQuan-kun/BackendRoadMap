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
                Bio = user.bio,
                AvatarUrl = user.avatar,
                CoverUrl = user.CoverUrl,
                Phone = user.Phone,
                Address = user.Address,
                BirthDate = user.BirthDate,
                Links = user.Links != null ? new UserLinksDto
                {
                    Github = user.Links.Github,
                    Portfolio = user.Links.Portfolio,
                    LinkedIn = user.Links.LinkedIn,
                    Facebook = user.Links.Facebook
                } : null,
                Skills = user.Skills,
                CompletedNodes = user.CompletedNodes,
                OnboardingResponses = user.OnboardingResponses,
                IsApproved = user.IsApproved
            };
        }
    }
}
