namespace BackendService.Models.DTOs.Application.Responses
{
    public class ApplicationDetailResponseDto
    {
        public string Id { get; set; } = string.Empty;
        public string JobId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;
        public InfoUser User { get; set; } = new();
        public InfoJob Job { get; set; } = new();
    }
    public class InfoUser: ListUserForApplicationResponseDto
    {
        public string FullName { get; set; } = string.Empty;
        public string AvatarUrl { get; set; } = string.Empty;
    }
    public class InfoJob: ListJobForApplicationResponseDto
    {
        public string Title { get; set; } = string.Empty;
        public string Location { get; set; } = string.Empty;
    }
}
