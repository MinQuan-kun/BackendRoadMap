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

using Microsoft.Extensions.DependencyInjection;

namespace BackendService.Services
{
    public class AuthService(IUserRepository userRepository, IConfiguration _config, IServiceScopeFactory scopeFactory): IAuthService
    {
        private readonly IUserRepository _userRepository = userRepository;
        private readonly IConfiguration _config = _config; 
        private readonly IServiceScopeFactory _scopeFactory = scopeFactory;


        public string GenerateToken(User user)
        {
            var jwtKey = _config["Jwt:Key"] ?? throw new Exception("JWT Key is missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
            new Claim(ClaimTypes.Role, user.Role.ToString())
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
            
            if (user == null)
            {
                user = await _userRepository.GetByUserNameAsync(request.Email, cancellationToken);
            }

            if (user == null || string.IsNullOrEmpty(user.PasswordHash) || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
            {
                return null;
            }
            var token = GenerateToken(user);

            return new AuthResponseDto
            {
                Token = token,
            };
        }

        public async Task ForgotPasswordAsync(string email, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null) return; // Theo plan: Trả về chung message bảo mật, không lỗi.

            var code = new Random().Next(100000, 999999).ToString();
            user.ResetPasswordCode = code;
            user.ResetPasswordCodeExpiry = DateTime.UtcNow.AddMinutes(15);
            await _userRepository.UpdateAsync(user.Id!, user, cancellationToken);

            // Gửi email ngầm
            Task.Factory.StartNew(async () =>
            {
                using var scope = _scopeFactory.CreateScope();
                var emailService = scope.ServiceProvider.GetRequiredService<IEmailService>();
                var dbContext = scope.ServiceProvider.GetRequiredService<Data.MongoDbContext>();

                var emailHistory = new EmailHistory
                {
                    Email = email,
                    Subject = "Reset Password Code",
                    Status = "Fail",
                    CreatedAt = DateTime.UtcNow
                };

                try
                {
                    // Lấy nội dung template (đơn giản, hoặc đọc từ wwwroot)
                    string htmlMessage = $@"
                        <h2>Reset Password</h2>
                        <p>Hi {user.UserName},</p>
                        <p>Your password reset code is: <strong>{code}</strong></p>
                        <p>This code will expire in 15 minutes.</p>
                    ";

                    var templatePath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "email-templates", "reset-password.html");
                    if (File.Exists(templatePath))
                    {
                        htmlMessage = await File.ReadAllTextAsync(templatePath);
                        htmlMessage = htmlMessage.Replace("{{UserName}}", user.UserName).Replace("{{Code}}", code);
                    }

                    await emailService.SendEmailAsync(email, "Reset Password Code", htmlMessage);
                    emailHistory.Status = "Success";
                }
                catch (Exception ex)
                {
                    emailHistory.Exceptions = ex.Message;
                }
                finally
                {
                    await dbContext.EmailHistories.InsertOneAsync(emailHistory);
                }
            }, TaskCreationOptions.LongRunning);
        }

        public async Task ResetPasswordAsync(string email, string code, string newPassword, CancellationToken cancellationToken = default)
        {
            var user = await _userRepository.GetByEmailAsync(email, cancellationToken);
            if (user == null || user.ResetPasswordCode != code)
                throw new Exception("Invalid code.");

            if (user.ResetPasswordCodeExpiry < DateTime.UtcNow)
                throw new Exception("Code has expired.");

            user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetPasswordCode = null;
            user.ResetPasswordCodeExpiry = null;

            await _userRepository.UpdateAsync(user.Id!, user, cancellationToken);
        }
    }
}
