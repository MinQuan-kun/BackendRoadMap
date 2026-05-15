using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BackendService.Data;
using BackendService.Models.Entities;
using Microsoft.AspNetCore.Authorization;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoadmapGraphsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public RoadmapGraphsController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("{id}")]
        public async Task<ActionResult> GetGraph(string id)
        {
            var graph = await _context.RoadmapGraphs.Find(g => g.Id == id).FirstOrDefaultAsync();
            if (graph == null) return NotFound();

            var nodes = await _context.RoadmapNodes.Find(n => n.GraphId == id).ToListAsync();
            var edges = await _context.RoadmapEdges.Find(e => e.GraphId == id).ToListAsync();

            return Ok(new
            {
                graph.Id,
                graph.Title,
                graph.GraphType,
                graph.LayoutType,
                Nodes = nodes.Select(n => new 
                {
                    n.Id,
                    n.NodeType,
                    n.ReferenceId,
                    n.Title,
                    Position = new { x = n.PositionX, y = n.PositionY }
                }).ToList(),
                Edges = edges.Select(e => new
                {
                    e.Id,
                    e.SourceNodeId,
                    e.TargetNodeId
                }).ToList()
            });
        }

        [Authorize(Roles = "Admin")]
        [HttpPost]
        public async Task<ActionResult<RoadmapGraph>> CreateGraph([FromBody] RoadmapGraph graph)
        {
            graph.Id = null;
            graph.CreatedAt = DateTime.UtcNow;
            await _context.RoadmapGraphs.InsertOneAsync(graph);
            return CreatedAtAction(nameof(GetGraph), new { id = graph.Id }, graph);
        }
    }
}
