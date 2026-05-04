namespace BackendService.Models.DTOs.User.Requests
{
    public class UpdateProfileRequestDto
    {
        public string? FullName { get; set; }
        public string? Bio { get; set; }
        public string? Phone { get; set; }
        public string? Address { get; set; }
        public string? BirthDate { get; set; }
        public string? AvatarUrl { get; set; }
        public string? CoverUrl { get; set; }
        public UpdateLinksDto? Links { get; set; }
        public List<string>? Skills { get; set; }
    }

    public class UpdateLinksDto
    {
        public string? Github { get; set; }
        public string? Portfolio { get; set; }
        public string? LinkedIn { get; set; }
        public string? Facebook { get; set; }
    }

    public class ChangePasswordRequestDto
    {
        public string OldPassword { get; set; } = null!;
        public string NewPassword { get; set; } = null!;
    }
}
