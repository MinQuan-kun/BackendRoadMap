using BackendService.Data;
using BackendService.Mapping;
using BackendService.Models.DTOs.User.Requests;
using BackendService.Models.DTOs.User.Responses;
using BackendService.Models.Entities;
using BCrypt.Net;
using Microsoft.AspNetCore.Authorization;
using BackendService.Services.Interface;
using FluentValidation;
using Microsoft.AspNetCore.Mvc;
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
        private readonly ICloudinaryService _cloudinaryService;

        public UsersController(MongoDbContext context, IConfiguration config, IValidator<RegisterRequestDto> registerRequest, IUserService userService, IValidator<LoginRequestDto> loginRequest, IAuthService authService, ICloudinaryService cloudinaryService)
        {
            _context = context;
            _config = config;
            _registerRequest = registerRequest;
            _userService = userService;
            _loginRequest = loginRequest;
            _authService = authService;
            _cloudinaryService = cloudinaryService;
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

                var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
                if (user == null)
                {
                    return NotFound();
                }

                return Ok(UserToUserResponseDto.Transform(user));
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        // ═══ Cập nhật Profile ═══════════════════════════
        [HttpPut("profile")]
        [Authorize]
        public async Task<ActionResult<UserResponseDto>> UpdateProfile([FromBody] UpdateProfileRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            // Cập nhật chỉ các trường được gửi (partial update)
            if (request.FullName != null) user.FullName = request.FullName;
            if (request.Bio != null) user.bio = request.Bio;
            if (request.Phone != null) user.Phone = request.Phone;
            if (request.Address != null) user.Address = request.Address;
            if (request.BirthDate != null) user.BirthDate = request.BirthDate;
            
            if (request.AvatarUrl != null && request.AvatarUrl != user.avatar)
            {
                if (!string.IsNullOrEmpty(user.avatar))
                {
                    var publicId = ExtractPublicId(user.avatar);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        await _cloudinaryService.DeleteImageAsync(publicId);
                    }
                }
                user.avatar = request.AvatarUrl;
            }

            if (request.CoverUrl != null && request.CoverUrl != user.CoverUrl)
            {
                if (!string.IsNullOrEmpty(user.CoverUrl))
                {
                    var publicId = ExtractPublicId(user.CoverUrl);
                    if (!string.IsNullOrEmpty(publicId))
                    {
                        await _cloudinaryService.DeleteImageAsync(publicId);
                    }
                }
                user.CoverUrl = request.CoverUrl;
            }

            if (request.Skills != null) user.Skills = request.Skills;

            if (request.Links != null)
            {
                user.Links ??= new UserLinks();
                if (request.Links.Github != null) user.Links.Github = request.Links.Github;
                if (request.Links.Portfolio != null) user.Links.Portfolio = request.Links.Portfolio;
                if (request.Links.LinkedIn != null) user.Links.LinkedIn = request.Links.LinkedIn;
                if (request.Links.Facebook != null) user.Links.Facebook = request.Links.Facebook;
            }

            await _context.Users.ReplaceOneAsync(u => u.Id == userId, user);

            return Ok(UserToUserResponseDto.Transform(user));
        }

        [HttpPost("profile/avatar")]
        [Authorize]
        public async Task<IActionResult> UploadAvatar(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                return BadRequest("Không có file nào được tải lên.");
            }

            try
            {
                var result = await _cloudinaryService.UploadImageAsync(file, "avatars");
                if (result.Error != null)
                {
                    return BadRequest(result.Error.Message);
                }

                return Ok(new { url = result.SecureUrl.ToString() });
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        private string? ExtractPublicId(string url)
        {
            try
            {
                var uri = new Uri(url);
                var segments = uri.Segments;
                
                int uploadIndex = -1;
                for (int i = 0; i < segments.Length; i++)
                {
                    if (segments[i].Equals("upload/", StringComparison.OrdinalIgnoreCase))
                    {
                        uploadIndex = i;
                        break;
                    }
                }

                if (uploadIndex == -1 || uploadIndex + 2 >= segments.Length) return null;
                
                var publicIdSegments = segments.Skip(uploadIndex + 2).Select(s => s.TrimEnd('/'));
                var publicIdWithExtension = string.Join("/", publicIdSegments);
                
                var lastDotIndex = publicIdWithExtension.LastIndexOf('.');
                if (lastDotIndex != -1)
                {
                    return publicIdWithExtension.Substring(0, lastDotIndex);
                }
                
                return publicIdWithExtension;
            }
            catch
            {
                return null;
            }
        }

        [HttpPut("change-password")]
        [Authorize]
        public async Task<IActionResult> ChangePassword([FromBody] ChangePasswordRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync();
            if (user == null)
            {
                return NotFound("Người dùng không tồn tại.");
            }

            if (!BCrypt.Net.BCrypt.Verify(request.OldPassword, user.Password))
            {
                return BadRequest(new { message = "Mật khẩu cũ không chính xác." });
            }

            if (string.IsNullOrWhiteSpace(request.NewPassword) || request.NewPassword.Length < 6)
            {
                return BadRequest(new { message = "Mật khẩu mới phải có ít nhất 6 ký tự." });
            }

            user.Password = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            await _context.Users.ReplaceOneAsync(u => u.Id == userId, user);

            return Ok(new { message = "Đổi mật khẩu thành công." });
        }

        [HttpGet("my-applications")]
        [Authorize]
        public async Task<ActionResult> GetMyApplications()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized();
            }

            var applications = await _context.Applications
                .Find(a => a.UserId == userId)
                .SortByDescending(a => a.AppliedAt)
                .ToListAsync();

            var jobIds = applications.Select(a => a.JobId).Distinct().ToList();
            var jobs = await _context.Jobs
                .Find(j => j.Id != null && jobIds.Contains(j.Id))
                .ToListAsync();
            var jobLookup = jobs.Where(j => j.Id != null).ToDictionary(j => j.Id!, j => j);

            var companyIds = jobs.Select(j => j.CompanyId).Where(id => !string.IsNullOrWhiteSpace(id)).Distinct().ToList();
            var companies = await _context.Companies
                .Find(c => c.Id != null && companyIds.Contains(c.Id))
                .ToListAsync();
            var companyLookup = companies.Where(c => c.Id != null).ToDictionary(c => c.Id!, c => c);

            var result = applications.Select(app =>
            {
                jobLookup.TryGetValue(app.JobId, out var job);
                Company? company = null;
                if (job != null && !string.IsNullOrWhiteSpace(job.CompanyId))
                {
                    companyLookup.TryGetValue(job.CompanyId, out company);
                }

                return new
                {
                    applicationId = app.Id,
                    jobId = app.JobId,
                    status = app.Status,
                    matchingScore = app.MatchingScore,
                    appliedAt = app.AppliedAt,
                    job = job != null ? new
                    {
                        id = job.Id,
                        title = job.Title,
                        location = job.Location,
                        salary = job.Salary,
                        skills = job.Skills,
                        experienceLevel = job.ExperienceLevel
                    } : null,
                    company = company != null ? new
                    {
                        name = company.CompanyName,
                        logo = company.LogoUrl
                    } : null
                };
            }).ToList();

            return Ok(result);
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