using BackendService.Repository.Interface;
using BackendService.Services.Interface;

namespace BackendService.Services
{
    public class JobService(IJobRepository jobRepository): IJobService
    {
        private readonly IJobRepository _jobRepository = jobRepository;
    }
}
