using BackendService.Models.DTOs.User;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using BackendService.Services.Interface;

namespace BackendService.Services
{
    public class AuthService(IUserRepository userRepository): IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;

        public string GenerateToken(User user)
        {
            throw new NotImplementedException();
        }

        public Task<AuthResponseDto> LoginAsync(LoginRequest request, CancellationToken cancellationToken = default)
        {
            throw new NotImplementedException();
        }
    }
}
