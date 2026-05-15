using Microsoft.AspNetCore.Mvc;
using BackendService.Services.Interface;
using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.DTOs.User.Responses;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UsersController : ControllerBase
    {
        private readonly IUserService _userService;
        private readonly IAuthService _authService;

        public UsersController(IUserService userService, IAuthService authService)
        {
            _userService = userService;
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<ActionResult<RegisterResponseDto>> Register([FromBody] RegisterRequestDto request, CancellationToken cancellationToken)
        {
            try
            {
                var response = await _userService.RegisterAsync(request, cancellationToken);
                return Ok(response);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponseDto>> Login([FromBody] LoginRequestDto request, CancellationToken cancellationToken)
        {
            var response = await _authService.LoginAsync(request, cancellationToken);
            if (response == null)
            {
                return Unauthorized("Tên đăng nhập hoặc mật khẩu không chính xác.");
            }
            return Ok(response);
        }

        [HttpGet("profile")]
        [Authorize]
        public async Task<ActionResult<ResponseUserByIdDto>> GetProfile(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            return Ok(user);
        }
    }
}
