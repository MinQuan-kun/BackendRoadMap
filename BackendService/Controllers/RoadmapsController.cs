using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using BackendService.Data;
using BackendService.Models.Entities;
using System.Security.Claims;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class RoadmapsController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public RoadmapsController(MongoDbContext context)
        {
            _context = context;
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult> SaveUserRoadmap([FromBody] UserRoadmapRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            // 1. Create RoadmapGraph
            var graph = new RoadmapGraph
            {
                Title = request.Title,
                GraphType = "community",
                OwnerUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _context.RoadmapGraphs.InsertOneAsync(graph);
            var graphId = graph.Id!;

            // 2. Create Nodes & Edges
            var nodeIds = new List<string>();
            var edgeIds = new List<string>();
            var tempToRealId = new Dictionary<string, string>();

            if (request.Nodes != null)
            {
                foreach (var n in request.Nodes)
                {
                    var node = new RoadmapNode
                    {
                        GraphId = graphId,
                        Title = n.Title,
                        NodeType = n.NodeType,
                        ReferenceId = n.ReferenceId,
                        PositionX = n.PositionX,
                        PositionY = n.PositionY
                    };
                    await _context.RoadmapNodes.InsertOneAsync(node);
                    nodeIds.Add(node.Id!);
                    tempToRealId[n.Id] = node.Id!;
                }
            }

            if (request.Edges != null)
            {
                foreach (var e in request.Edges)
                {
                    if (tempToRealId.TryGetValue(e.SourceNodeId, out var src) && 
                        tempToRealId.TryGetValue(e.TargetNodeId, out var dst))
                    {
                        var edge = new RoadmapEdge
                        {
                            GraphId = graphId,
                            SourceNodeId = src,
                            TargetNodeId = dst
                        };
                        await _context.RoadmapEdges.InsertOneAsync(edge);
                        edgeIds.Add(edge.Id!);
                    }
                }
            }

            // Update graph with node/edge IDs
            await _context.RoadmapGraphs.UpdateOneAsync(
                g => g.Id == graphId,
                Builders<RoadmapGraph>.Update.Set(g => g.NodeIds, nodeIds).Set(g => g.EdgeIds, edgeIds)
            );

            // 3. Create Pathway so it shows up in "My Roadmaps"
            var pathway = new Pathway
            {
                Title = request.Title,
                Slug = request.Title.ToLower().Replace(" ", "-") + "-" + Guid.NewGuid().ToString().Substring(0, 8),
                Description = "Roadmap được tạo bởi người dùng.",
                IsOfficial = false,
                CreatedBy = userId,
                RoadmapGraphId = graphId,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Pathways.InsertOneAsync(pathway);

            return Ok(new { id = pathway.Id, graphId = graphId });
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult> UpdateUserRoadmap(string id, [FromBody] UserRoadmapRequestDto request)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var pathway = await _context.Pathways.Find(p => p.Id == id && p.CreatedBy == userId).FirstOrDefaultAsync();
            if (pathway == null) return NotFound("Lộ trình không tồn tại hoặc bạn không có quyền chỉnh sửa.");

            var graphId = pathway.RoadmapGraphId;
            if (string.IsNullOrEmpty(graphId)) return BadRequest("Không tìm thấy dữ liệu đồ thị.");

            // Update Pathway title
            await _context.Pathways.UpdateOneAsync(
                p => p.Id == id,
                Builders<Pathway>.Update.Set(p => p.Title, request.Title)
            );

            // Update Graph title
            await _context.RoadmapGraphs.UpdateOneAsync(
                g => g.Id == graphId,
                Builders<RoadmapGraph>.Update.Set(g => g.Title, request.Title)
            );

            // Delete old nodes/edges
            await _context.RoadmapNodes.DeleteManyAsync(n => n.GraphId == graphId);
            await _context.RoadmapEdges.DeleteManyAsync(e => e.GraphId == graphId);

            // Create new nodes/edges
            var nodeIds = new List<string>();
            var edgeIds = new List<string>();
            var tempToRealId = new Dictionary<string, string>();

            if (request.Nodes != null)
            {
                foreach (var n in request.Nodes)
                {
                    var node = new RoadmapNode
                    {
                        GraphId = graphId,
                        Title = n.Title,
                        NodeType = n.NodeType,
                        ReferenceId = n.ReferenceId,
                        PositionX = n.PositionX,
                        PositionY = n.PositionY
                    };
                    await _context.RoadmapNodes.InsertOneAsync(node);
                    nodeIds.Add(node.Id!);
                    tempToRealId[n.Id] = node.Id!;
                }
            }

            if (request.Edges != null)
            {
                foreach (var e in request.Edges)
                {
                    if (tempToRealId.TryGetValue(e.SourceNodeId, out var src) && 
                        tempToRealId.TryGetValue(e.TargetNodeId, out var dst))
                    {
                        var edge = new RoadmapEdge
                        {
                            GraphId = graphId,
                            SourceNodeId = src,
                            TargetNodeId = dst
                        };
                        await _context.RoadmapEdges.InsertOneAsync(edge);
                        edgeIds.Add(edge.Id!);
                    }
                }
            }

            // Update graph with new node/edge IDs
            await _context.RoadmapGraphs.UpdateOneAsync(
                g => g.Id == graphId,
                Builders<RoadmapGraph>.Update.Set(g => g.NodeIds, nodeIds).Set(g => g.EdgeIds, edgeIds)
            );

            return Ok(new { message = "Cập nhật thành công!" });
        }
    }

    public class UserRoadmapRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public List<UserNodeDto> Nodes { get; set; } = new();
        public List<UserEdgeDto> Edges { get; set; } = new();
    }

    public class UserNodeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string NodeType { get; set; } = "default";
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public string? ReferenceId { get; set; }
    }

    public class UserEdgeDto
    {
        public string SourceNodeId { get; set; } = string.Empty;
        public string TargetNodeId { get; set; } = string.Empty;
    }
}
