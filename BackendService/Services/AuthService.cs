using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.DTOs.User.Responses;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using BackendService.Services.Interface;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace BackendService.Services
{
    public class AuthService(IUserRepository userRepository, IConfiguration _config): IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IConfiguration _config = _config; 


        public string GenerateToken(User user)
        {
            var jwtKey = _config["Jwt:Key"] ?? throw new Exception("JWT Key is missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id!)
        }),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };
            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        public async Task<AuthResponseDto?> LoginAsync(LoginRequestDto request, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email, cancellationToken);
            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return null;
            }
            var token = GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
            };
        }
    }
}
