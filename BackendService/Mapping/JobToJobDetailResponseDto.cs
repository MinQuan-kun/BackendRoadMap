using BackendService.Models.DTOs.Job.Responses;
using BackendService.Models.Entities;

namespace BackendService.Mapping
{
    public static class JobToJobDetailResponseDto
    {
        public static JobDetailResponseDto Transform(Job job)
        {
            return new JobDetailResponseDto
            {
                CompanyId = job.CompanyId,
                Title = job.Title,
                Description = job.Description,
                Location = job.Location,
                Salary = job.Salary,
                Skills = job.Skills,
                ExperienceLevel = job.ExperienceLevel,
                MatchingRate = job.MatchingRate,
                Company = new()
                {
                    Name = job.Company?.CompanyName ?? string.Empty,
                    LogoURL = job.Company?.LogoUrl ?? string.Empty,
                }
            };
        }
    }
}
