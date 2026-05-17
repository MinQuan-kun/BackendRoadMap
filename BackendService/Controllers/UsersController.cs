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
        private readonly ICloudinaryService _cloudinaryService;

        public UsersController(IUserService userService, IAuthService authService, ICloudinaryService cloudinaryService)
        {
            _userService = userService;
            _authService = authService;
            _cloudinaryService = cloudinaryService;
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

        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult> UpdateProfile([FromBody] UpdateProfileRequestDto request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _userService.GetUserByIdAsync(userId, cancellationToken);
            if (user == null) return NotFound();

            await _userService.UpdateProfileAsync(userId, request, cancellationToken);
            var updatedUser = await _userService.GetUserByIdAsync(userId, cancellationToken);
            return Ok(updatedUser);
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<ActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try {
                await _userService.ChangePasswordAsync(userId, request.OldPassword, request.NewPassword, cancellationToken);
                return Ok(new { message = "Đổi mật khẩu thành công." });
            } catch (Exception ex) {
                return BadRequest(ex.Message);
            }
        }

        [HttpDelete("profile")]
        [Authorize]
        public async Task<ActionResult> DeleteProfile(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            await _userService.DeleteAccountAsync(userId, cancellationToken);
            return Ok(new { message = "Xóa tài khoản thành công." });
        }

        [HttpPost("progress")]
        [Authorize]
        public async Task<ActionResult<ResponseUserByIdDto>> UpdateProgress([FromBody] ProgressRequestDto request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var result = await _userService.UpdateProgressAsync(userId, request.NodeId, request.Status, cancellationToken);
            return Ok(new { data = new { completed = result.CompletedNodes, skipped = result.SkippedNodes } });
        }

        [HttpPost("profile/avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file, CancellationToken cancellationToken)
        {
            if (file == null || file.Length == 0) return BadRequest("File không hợp lệ.");

            var result = await _cloudinaryService.UploadImageAsync(file, "profile");
            if (result.Error != null) return BadRequest(result.Error.Message);

            return Ok(new { url = result.SecureUrl.ToString(), publicId = result.PublicId });
        }
    }

    public class ProgressRequestDto
    {
        public string NodeId { get; set; } = string.Empty;
        public string Status { get; set; } = "completed"; // completed, skipped, none
    }
}
