using BackendService.Models.DTOs.Job.Responses;
using BackendService.Models.Entities;

namespace BackendService.Mapping
{
    public static class JobToJobListResponseDto
    {
        public static JobListResponsedto Transform(Job job) 
        {
            return new JobListResponsedto
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.Company?.CompanyName ?? "Unknown Company",
                Salary = job.Salary,
                Skills = job.Skills,
            };

        }
    }
}
