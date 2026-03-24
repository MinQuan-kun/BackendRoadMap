using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using BackendService.Data;
using BackendService.Models.DTOs.User;
using BackendService.Models.Entities;
using BCrypt.Net;
using Microsoft.AspNetCore.Mvc;
using Microsoft.IdentityModel.Tokens;
using MongoDB.Driver;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _config;

        public UsersController(MongoDbContext context, IConfiguration config)
        {
            _context = context;
            _config = config;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterRequest>> Register(RegisterRequest request)
        {
            // Kiểm tra Email tồn tại
            var emailExists = await _context.Users.Find(u => u.Email == request.Email).AnyAsync();
            if (emailExists) return BadRequest("Email này đã được đăng ký bởi một tài khoản khác.");

            // Kiểm tra UserName đã tồn tại chưa
            var userExists = await _context.Users.Find(u => u.UserName == request.UserName).AnyAsync();
            if (userExists) return BadRequest("Tên đăng nhập này đã tồn tại.");

            var user = new User
            {
                UserName = request.UserName,
                Email = request.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(request.Password),
                Role = 1,
                CreateAt = DateTime.UtcNow
            };

            await _context.Users.InsertOneAsync(user);
            return Ok(MapToResponse(user));
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequest request)
        {
            var user = await _context.Users.Find(u => u.Email == request.Email).FirstOrDefaultAsync();

            if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.Password))
            {
                return Unauthorized("Email hoặc mật khẩu không chính xác.");
            }

            var token = CreateToken(user);

            return Ok(new AuthResponseDto
            {
                Token = token,
                User = MapToResponse(user)
            });
        }

        private string CreateToken(User user)
        {
            var jwtKey = _config["Jwt:Key"] ?? throw new Exception("JWT Key is missing");
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256Signature);

            var tokenDescriptor = new SecurityTokenDescriptor
            {
                Subject = new ClaimsIdentity(new[] {
            new Claim(ClaimTypes.NameIdentifier, user.Id!),
            new Claim(ClaimTypes.Name, user.UserName)
        }),
                Expires = DateTime.Now.AddDays(7),
                SigningCredentials = creds
            };

            var tokenHandler = new JwtSecurityTokenHandler();
            return tokenHandler.WriteToken(tokenHandler.CreateToken(tokenDescriptor));
        }

        [HttpPut("{id}/onboarding")]
        public async Task<IActionResult> SaveOnboarding(string id, [FromBody] OnboardingRequest request)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound("Người dùng không tồn tại.");

            user.OnboardingResponses = request.Responses;

            await _context.Users.ReplaceOneAsync(u => u.Id == id, user);
            return Ok(new { message = "Lưu khảo sát thành công", data = MapToResponse(user) });
        }

        private static UserResponseDto MapToResponse(User user)
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