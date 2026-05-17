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
                DisplayName = user.DisplayName,
                Email = user.Email,
                Role = user.Role,
                Status = user.Status,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
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
                SkillTags = user.SkillTags,
                IsRecruiterVerified = user.IsRecruiterVerified,
                IsApproved = user.IsRecruiterVerified,
                CreatedAt = user.CreatedAt
            };
        }
    }
}
