using BackendService.Models.DTOs.Application.Responses;
using BackendService.Models.Entities;

namespace BackendService.Mapping
{
    public static class ApplicationToApplicationResponseDto
    {
        public static ApplicationResponseDto Transform(Application app)
        {
            return new ApplicationResponseDto
            {
                Id = app.Id,
                JobId = app.JobId,
                UserId = app.UserId,
                MatchingScore = app.MatchingScore,
                Status = app.Status,
                User = new()
                {
                    Username = app.User?.UserName ?? string.Empty,
                    Email = app.User?.Email ?? string.Empty
                },
                Job = new()
                {
                    CompanyId = app.Job?.CompanyId ?? string.Empty,
                    Company = new()
                    {
                        CompanyName = app.Job?.Company?.CompanyName ?? string.Empty
                    }
                }
            };

        }

    }
}
