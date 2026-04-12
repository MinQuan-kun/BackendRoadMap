using BackendService.Data;
using BackendService.Mapping;
using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.DTOs.User.Responses;
using BackendService.Models.Entities;
using BackendService.Services.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using MongoDB.Driver;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly IConfiguration _config;
        private readonly IValidator<RegisterRequestDto> _registerRequest;
        private readonly IValidator<LoginRequestDto> _loginRequest;
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UsersController(MongoDbContext context, IConfiguration config, IValidator<RegisterRequestDto> registerRequest, IUserService userService, IValidator<LoginRequestDto> loginRequest, IAuthService authService)
        {
            _context = context;
            _config = config;
            _registerRequest = registerRequest;
            _userService = userService;
            _loginRequest = loginRequest;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> RegisterAsync(RegisterRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _registerRequest.ValidateAsync(request);
                if (validationResult != null && !validationResult.IsValid)
                {
                    return BadRequest();
                }
                var result = await _userService.RegisterAsync(request, cancellationToken);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login(LoginRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var validationResult = await _loginRequest.ValidateAsync(request);
                if (validationResult != null && !validationResult.IsValid)
                {
                    return BadRequest();
                }
                var result = await _authService.LoginAsync(request, cancellationToken);
                if (result == null)
                {
                    return Unauthorized(new { message = "Tài khoản hoặc mật khẩu không chính xác." });
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserById(string id, CancellationToken cancellationToken)
        {
            try
            {
                var result = await _userService.GetUserByIdAsync(id, cancellationToken);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> GetProfile(CancellationToken cancellationToken)
        {
            try
            {
                var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    return Unauthorized();
                }

                var result = await _userService.GetUserByIdAsync(userId, cancellationToken);
                if (result == null)
                {
                    return NotFound();
                }
                return Ok(result);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpPut("{id}/onboarding")]
        public async Task<IActionResult> SaveOnboarding(string id, [FromBody] OnboardingRequest request)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound("Người dùng không tồn tại.");

            user.OnboardingResponses = request.Responses;

            await _context.Users.ReplaceOneAsync(u => u.Id == id, user);
            return Ok(new { message = "Lưu khảo sát thành công", data = UserToUserResponseDto.Transform(user) });
        }
    }
}