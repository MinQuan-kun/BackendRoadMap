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
               UserName = user.UserName,
               Email = user.Email,
               AvatarUrl = user.avatar,
               Fullname = user.FullName,    
               Bio = user.bio,
               Role = user.Role



            };
        }
    }
}
