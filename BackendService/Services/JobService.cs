using BackendService.Mapping;
using BackendService.Models.DTOs.Job.Responses;
using BackendService.Repository.Interface;
using BackendService.Services.Interface;

namespace BackendService.Services
{
    public class JobService(IJobRepository jobRepository): IJobService
    {
        private readonly IJobRepository _jobRepository = jobRepository;

        public async Task<JobDetailResponseDto> GetByIdAsync(string Id, CancellationToken cancellationToken = default)
        {
            var job = await _jobRepository.GetByIdAsync(Id, cancellationToken);
            if (job == null)
            {
                throw new Exception("Cannot find Job");
            }
            return JobToJobDetailResponseDto.Transform(job);
        }

        public async Task<List<JobListResponsedto>> GetListAsync(CancellationToken cancellationToken = default)
        {
            var jobs = await _jobRepository.GetListAsync(cancellationToken);
            if (jobs == null)
            {
                throw new Exception("No Jobs found.");
            }
            var mappedJobs = jobs.Select(JobToJobListResponseDto.Transform).ToList();
            return mappedJobs;
        }
    }
}
