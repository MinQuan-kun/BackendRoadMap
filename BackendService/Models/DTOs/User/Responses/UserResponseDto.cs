namespace BackendService.Models.DTOs.User.Responses
{
    public class UserResponseDto
    {
        public string? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string? FullName { get; set; }
        public string Email { get; set; } = string.Empty;
        public int Role { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? BirthDate { get; set; }
        public UserLinksDto? Links { get; set; }
        public List<string> Skills { get; set; } = new();
        public List<string> CompletedNodes { get; set; } = new();
        public List<string> SkippedNodes { get; set; } = new();
        public List<string> InterestedNodes { get; set; } = new();
        public Dictionary<string, string> QuizResponses { get; set; } = new();
        public bool HasCompletedQuiz { get; set; }
        public bool IsApproved { get; set; }
    }

    public class UserLinksDto
    {
        public string? Github { get; set; }
        public string? Portfolio { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }
    }
}
