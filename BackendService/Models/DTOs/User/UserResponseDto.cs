namespace BackendService.Models.DTOs.User
{
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
