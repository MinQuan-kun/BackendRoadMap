using BackendService.Models.DTOs.User.Responses;
using BackendService.Models.Entities;

namespace BackendService.Mapping
{
    public static class UserToResponseUserById
    {
        public static ResponseUserByIdDto Transform(User user)
        {
            return new ResponseUserByIdDto
            {
                Id = user.Id,
                UserName = user.UserName,
                Email = user.Email,
                DisplayName = user.DisplayName,
                Role = user.Role,
                Bio = user.Bio,
                AvatarUrl = user.AvatarUrl,
                CoverUrl = user.CoverUrl,
                Links = user.Links,
                Phone = user.Phone,
                Address = user.Address,
                BirthDate = user.BirthDate,
                Skills = user.SkillTags,
                CompletedNodes = user.CompletedNodes,
                SkippedNodes = user.SkippedNodes,
                FollowedPathwayIds = user.FollowedPathwayIds,
                IsRecruiterVerified = user.IsRecruiterVerified,
                IsApproved = user.IsRecruiterVerified
            };
        }
    }
}
