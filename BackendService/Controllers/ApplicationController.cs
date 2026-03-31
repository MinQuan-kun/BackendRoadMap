using BackendService.Models.DTOs.Application.Responses;
using BackendService.Services.Interface;
using Microsoft.AspNetCore.Mvc;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/Application")]
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
    }
}
