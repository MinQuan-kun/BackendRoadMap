using BackendService.Mapping;
using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.DTOs.User.Responses;
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
            var user = await _userRepository.CreateAsync(mappedUser, cancellationToken);
            var responseUser = UserToRegisterResponseDto.Transform(user);
            return responseUser;
        }
    }
}
