using BackendService.Models.DTOs.Application.Responses;
using BackendService.Models.Entities;

namespace BackendService.Mapping
{
    public static class ApplicationToApplicationDetailResponseDto
    {
        public static ApplicationDetailResponseDto Transform(Application app)
        {
            return new ApplicationDetailResponseDto
            {
                Id = app.Id,
                JobId = app.JobId,
                UserId = app.UserId,
                User = new()
                {
                    Username = app.User?.UserName ?? string.Empty,
                    Email = app.User?.Email ?? string.Empty,
                    FullName = app.User?.FullName ?? string.Empty,
                    AvatarUrl = app.User?.avatar ?? string.Empty
                },
                Job = new()
                {
                    CompanyId = app.Job?.CompanyId ?? string.Empty,
                    Title = app.Job?.Title ?? string.Empty,
                    Location = app.Job?.Location ?? string.Empty,
                }
            };
        }
    }
}
