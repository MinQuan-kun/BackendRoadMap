namespace BackendService.Models.DTOs.User.Responses
{
    public class ResponseUserByIdDto
    {
        public string? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public string? Fullname { get; set; }
        public int Role { get; set; }
        public string? Bio { get; set; }
        public string? AvatarUrl { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public List<string> Skills { get; set; } = new();
        public List<string> CompletedNodes { get; set; } = new();
    }
}
