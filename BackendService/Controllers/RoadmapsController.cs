using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Models.DTOs;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoadmapController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public RoadmapController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoadmapResponseDto>> GetRoadmap(string id)
        {
            var roadmap = await _context.Roadmaps.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (roadmap == null) return NotFound("Không tìm thấy Roadmap!");

            var nodeIds = roadmap.NodesLayout.Select(nl => nl.NodeId).ToList();

            var nodesData = await _context.Nodes.Find(n => nodeIds.Contains(n.Id)).ToListAsync();

            var response = new RoadmapResponseDto
            {
                Id = roadmap.Id,
                Title = roadmap.Title,
                Nodes = nodesData.Select(n => new FlowNodeDto
                {
                    Id = n.Id,
                    Position = new FlowPosition
                    {
                        X = roadmap.NodesLayout.FirstOrDefault(l => l.NodeId == n.Id)?.X ?? 0,
                        Y = roadmap.NodesLayout.FirstOrDefault(l => l.NodeId == n.Id)?.Y ?? 0
                    },
                    Data = new FlowData { Label = n.Name }
                }).ToList(),

                Edges = nodesData.Where(n => n.ParentId != null).Select(n => new FlowEdgeDto
                {
                    Id = $"e-{n.ParentId}-{n.Id}",
                    Source = n.ParentId!,
                    Target = n.Id!
                }).ToList()
            };

            return Ok(response);
        }
        [HttpGet("test-db")]
        public async Task<IActionResult> TestConnection()
        {
            try
            {
                // Thử đếm số lượng Node trong DB để check kết nối
                var count = await _context.Nodes.CountDocumentsAsync(_ => true);
                return Ok(new
                {
                    message = "✅ Kết nối MongoDB thành công!",
                    totalNodes = count,
                    time = DateTime.Now
                });
            }
            catch (Exception ex)
            {
                return BadRequest(new
                {
                    message = "❌ Kết nối thất bại!",
                    error = ex.Message
                });
            }
        }
    }
}