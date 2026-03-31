using BackendService.Mapping;
using BackendService.Models.DTOs.Application.Responses;
using BackendService.Repository.Interface;
using BackendService.Services.Interface;

namespace BackendService.Services
{
    public class ApplicationService(IApplicationRepository applicationRepository) : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository = applicationRepository;

        public async Task<List<ApplicationResponseDto>> GetListAsync(CancellationToken cancellationToken)
        {
             var listApplication = await  _applicationRepository.GetListAsync(cancellationToken);
            if (listApplication == null)
            {
                throw new Exception("No applications found.");
            }
            var mappedApplication = listApplication.Select(ApplicationToApplicationResponseDto.Transform).ToList();
            return mappedApplication;

        }
    }
}
