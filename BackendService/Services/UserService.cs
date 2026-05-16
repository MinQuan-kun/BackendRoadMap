using BackendService.Mapping;
using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.DTOs.User.Responses;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using BackendService.Services.Interface;

namespace BackendService.Services
{
    public class UserService(IUserRepository userRepository) : IUserService
    {
        public readonly IUserRepository _userRepository = userRepository;

        public async Task<ResponseUserByIdDto> GetUserByIdAsync(string id, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(id, cancellationToken);  
            var responseUser = UserToResponseUserById.Transform(user);
            return responseUser;

        }

        public async Task<RegisterResponseDto> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
        {
            var emailExists = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (emailExists != null)
            {
                throw new Exception("Email này đã được đăng ký bởi một tài khoản khác.");
            }
            var userExists = await _userRepository.GetByUserNameAsync(request.UserName, cancellationToken);
            if (userExists != null)
            {
                throw new Exception("Tên đăng nhập này đã tồn tại.");
            }
            var mappedUser = RegisterRequestDtoToUser.Transform(request);
            mappedUser.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password);
            var user = await _userRepository.CreateAsync(mappedUser, cancellationToken);
            var responseUser = UserToRegisterResponseDto.Transform(user);
            return responseUser;
        }

        public async Task<ResponseUserByIdDto> UpdateProgressAsync(string userId, string nodeId, string status, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) throw new Exception("Không tìm thấy người dùng");

            if (user.CompletedNodes == null) user.CompletedNodes = new List<string>();
            if (user.SkippedNodes == null) user.SkippedNodes = new List<string>();

            // Clean up existing
            user.CompletedNodes.Remove(nodeId);
            user.SkippedNodes.Remove(nodeId);

            if (status == "completed") user.CompletedNodes.Add(nodeId);
            else if (status == "skipped") user.SkippedNodes.Add(nodeId);

            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(user.Id!, user, cancellationToken);

            return UserToResponseUserById.Transform(user);
        }

        public async Task UpdateProfileAsync(string userId, UpdateProfileRequestDto request, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) throw new Exception("Không tìm thấy người dùng");

            if (!string.IsNullOrEmpty(request.FullName)) user.DisplayName = request.FullName;
            if (!string.IsNullOrEmpty(request.Bio)) user.Bio = request.Bio;
            if (!string.IsNullOrEmpty(request.AvatarUrl)) user.AvatarUrl = request.AvatarUrl;
            if (!string.IsNullOrEmpty(request.CoverUrl)) user.CoverUrl = request.CoverUrl;
            if (!string.IsNullOrEmpty(request.Phone)) user.Phone = request.Phone;
            if (!string.IsNullOrEmpty(request.Address)) user.Address = request.Address;
            if (!string.IsNullOrEmpty(request.BirthDate)) user.BirthDate = request.BirthDate;

            if (request.Links != null)
            {
                if (user.Links == null) user.Links = new UserLinks();
                user.Links.Github = request.Links.Github;
                user.Links.Portfolio = request.Links.Portfolio;
                user.Links.LinkedIn = request.Links.LinkedIn;
                user.Links.Facebook = request.Links.Facebook;
            }

            if (request.Skills != null)
            {
                user.SkillTags = request.Skills;
            }

            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(userId, user, cancellationToken);
        }

        public async Task ChangePasswordAsync(string userId, string oldPassword, string newPassword, CancellationToken cancellationToken)
        {
            var user = await _userRepository.GetByIdAsync(userId, cancellationToken);
            if (user == null) throw new Exception("Không tìm thấy người dùng");

            if (!BCrypt.Net.BCrypt.Verify(oldPassword, user.PasswordHash))
            {
                throw new Exception("Mật khẩu cũ không chính xác.");
            }

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.UpdatedAt = DateTime.UtcNow;
            await _userRepository.UpdateAsync(userId, user, cancellationToken);
        }

        public async Task DeleteAccountAsync(string userId, CancellationToken cancellationToken)
        {
            await _userRepository.DeleteAsync(userId, cancellationToken);
        }
    }
}
