namespace BackendService.Models.DTOs.User
{
    public class RegisterResponseDto
    {
        public string? Id { get; set; }
        public string UserName { get; set; } = string.Empty;
        public int Role { get; set; }
    }
}
