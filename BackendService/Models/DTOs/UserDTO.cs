namespace BackendService.Models.DTOs;

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

