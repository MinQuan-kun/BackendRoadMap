using BackendService.Models.DTOs.Application.Responses;
using BackendService.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/applications")]
    public class ApplicationController(IApplicationService applicationService) : Controller
    {
        private readonly IApplicationService _applicationService = applicationService;

        [HttpGet("list")]
        public async Task<ActionResult<List<ApplicationResponseDto>>> GetListApplicationsAsync(CancellationToken cancellationToken)
        {
            try
            {
                var applications = await _applicationService.GetListAsync(cancellationToken);
                return Ok(applications);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<ApplicationDetailResponseDto>> GetApplicationDetailAsync([FromQuery]string id, CancellationToken cancellationToken)
        {
            try
            {
                var application = await _applicationService.GetDetailAsync(id, cancellationToken);
                return Ok(application);
            }
            catch (Exception ex)
            {
                return StatusCode(StatusCodes.Status500InternalServerError, new { error = ex.Message });
            }
        }
    }
}
