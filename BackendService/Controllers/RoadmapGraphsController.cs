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
            if (graph == null && id.Length == 24 && System.Text.RegularExpressions.Regex.IsMatch(id, @"^[0-9a-fA-F]{24}$"))
            {
                var job = await _context.Jobs.Find(j => j.RoadmapGraphId == id || j.Id == id).FirstOrDefaultAsync();
                if (job != null && job.Title.Contains("Unreal", StringComparison.OrdinalIgnoreCase))
                {
                    var unrealGraph = await _context.RoadmapGraphs.Find(g => g.Title.Contains("Unreal")).FirstOrDefaultAsync();
                    if (unrealGraph != null)
                    {
                        id = unrealGraph.Id!;
                        graph = unrealGraph;
                    }
                }
                
                if (graph == null)
                {
                    var unityGraph = await _context.RoadmapGraphs.Find(g => g.Title.Contains("Unity")).FirstOrDefaultAsync();
                    if (unityGraph != null)
                    {
                        id = unityGraph.Id!;
                        graph = unityGraph;
                    }
                }

                if (graph == null)
                {
                    var firstGraph = await _context.RoadmapGraphs.Find(_ => true).FirstOrDefaultAsync();
                    if (firstGraph != null)
                    {
                        id = firstGraph.Id!;
                        graph = firstGraph;
                    }
                }
            }

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
