namespace BackendService.Models.DTOs.User.Requests
{
    public class UpdateProgressRequestDto
    {
        public string NodeId { get; set; } = null!;
        public string Status { get; set; } = null!; // "completed", "skipped", "not_started"
    }
}
