namespace BackendService.Models.DTOs.User.Responses
{
    public class UserResponseDto
    {
        public string? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public List<string> CompletedNodes { get; set; } = new();
        public Dictionary<string, string> OnboardingResponses { get; set; } = new();
    }
}
