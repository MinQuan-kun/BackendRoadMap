using BackendService.Data;
using BackendService.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class UsersController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public UsersController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers(CancellationToken cancellationToken)
        {
            var users = await _context.Users
                .Find(Builders<User>.Filter.Empty)
                .ToListAsync(cancellationToken);

            var response = users.Select(MapToResponse);

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<UserResponseDto>> GetUserById(string id, CancellationToken cancellationToken)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest(new { message = "Invalid user id format." });
            }

            var user = await _context.Users
                .Find(u => u.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return NotFound(new { message = "User not found." });
            }

            return Ok(MapToResponse(user));
        }

        [HttpPost]
        public async Task<ActionResult<UserResponseDto>> CreateUser(
            [FromBody] CreateUserRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.UserName) ||
                string.IsNullOrWhiteSpace(request.Password) ||
                string.IsNullOrWhiteSpace(request.FullName) ||
                string.IsNullOrWhiteSpace(request.Email))
            {
                return BadRequest(new { message = "UserName, Password, FullName, and Email are required." });
            }

            var emailExists = await _context.Users
                .Find(u => u.Email == request.Email)
                .AnyAsync(cancellationToken);

            if (emailExists)
            {
                return Conflict(new { message = "Email already exists." });
            }

            var userNameExists = await _context.Users
                .Find(u => u.UserName == request.UserName)
                .AnyAsync(cancellationToken);

            if (userNameExists)
            {
                return Conflict(new { message = "UserName already exists." });
            }

            var user = new User
            {
                UserName = request.UserName.Trim(),
                Password = request.Password,
                FullName = request.FullName.Trim(),
                Email = request.Email.Trim(),
                Role = request.Role,
                CompletedNodes = request.CompletedNodes ?? new List<string>(),
                OnboardingResponses = request.OnboardingResponses ?? new Dictionary<string, string>()
            };

            await _context.Users.InsertOneAsync(user, cancellationToken: cancellationToken);

            return CreatedAtAction(
                nameof(GetUserById),
                new { id = user.Id },
                MapToResponse(user));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<UserResponseDto>> UpdateUser(
            string id,
            [FromBody] UpdateUserRequest request,
            CancellationToken cancellationToken)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest(new { message = "Invalid user id format." });
            }

            var user = await _context.Users
                .Find(u => u.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (user is null)
            {
                return NotFound(new { message = "User not found." });
            }

            if (!string.IsNullOrWhiteSpace(request.UserName) && request.UserName != user.UserName)
            {
                var userNameExists = await _context.Users
                    .Find(u => u.UserName == request.UserName && u.Id != id)
                    .AnyAsync(cancellationToken);
                if (userNameExists)
                {
                    return Conflict(new { message = "UserName already exists." });
                }
                user.UserName = request.UserName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Email) && request.Email != user.Email)
            {
                var emailExists = await _context.Users
                    .Find(u => u.Email == request.Email && u.Id != id)
                    .AnyAsync(cancellationToken);
                if (emailExists)
                {
                    return Conflict(new { message = "Email already exists." });
                }
                user.Email = request.Email.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.FullName))
            {
                user.FullName = request.FullName.Trim();
            }

            if (!string.IsNullOrWhiteSpace(request.Password))
            {
                user.Password = request.Password;
            }

            if (request.Role.HasValue)
            {
                user.Role = request.Role.Value;
            }

            if (request.CompletedNodes is not null)
            {
                user.CompletedNodes = request.CompletedNodes;
            }

            if (request.OnboardingResponses is not null)
            {
                user.OnboardingResponses = request.OnboardingResponses;
            }

            await _context.Users.ReplaceOneAsync(u => u.Id == id, user, cancellationToken: cancellationToken);

            return Ok(MapToResponse(user));
        }

        [HttpDelete("{id}")]
        public async Task<ActionResult> DeleteUser(string id, CancellationToken cancellationToken)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest(new { message = "Invalid user id format." });
            }

            var deleteResult = await _context.Users.DeleteOneAsync(u => u.Id == id, cancellationToken);
            if (deleteResult.DeletedCount == 0)
            {
                return NotFound(new { message = "User not found." });
            }

            return NoContent();
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

    public class CreateUserRequest
    {
        public string UserName { get; set; } = string.Empty;
        public string Password { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; } = 1;
        public List<string>? CompletedNodes { get; set; }
        public Dictionary<string, string>? OnboardingResponses { get; set; }
    }

    public class UpdateUserRequest
    {
        public string? UserName { get; set; }
        public string? Password { get; set; }
        public string? FullName { get; set; }
        public string? Email { get; set; }
        public int? Role { get; set; }
        public List<string>? CompletedNodes { get; set; }
        public Dictionary<string, string>? OnboardingResponses { get; set; }
    }

    public class UserResponseDto
    {
        public string? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string FullName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public List<string> CompletedNodes { get; set; } = new();
        public Dictionary<string, string> OnboardingResponses { get; set; } = new();
    }
}
