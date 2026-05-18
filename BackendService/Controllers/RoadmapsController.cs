using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using BackendService.Services.Interface;
using System.Security.Claims;
using BackendService.Models.DTOs.Roadmap;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoadmapsController : ControllerBase
    {
        private readonly IRoadmapService _roadmapService;

        public RoadmapsController(IRoadmapService roadmapService)
        {
            _roadmapService = roadmapService;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> SaveUserRoadmap([FromBody] UserRoadmapRequestDto request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var (pathwayId, graphId) = await _roadmapService.SaveUserRoadmapAsync(userId, request, cancellationToken);
            return Ok(new { id = pathwayId, graphId = graphId });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUserRoadmap(string id, [FromBody] UserRoadmapRequestDto request, CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            try
            {
                await _roadmapService.UpdateUserRoadmapAsync(userId, id, request, cancellationToken);
                return Ok(new { message = "Cập nhật thành công!" });
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }
}
