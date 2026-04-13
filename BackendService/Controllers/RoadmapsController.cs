using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using MongoDB.Bson;
using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Models.DTOs;
using Microsoft.AspNetCore.Authorization;
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

        // Lấy toàn bộ roadmap
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoadmapSummaryDto>>> GetAllRoadmaps([FromQuery] string? creatorId)
        {
            var filter = string.IsNullOrWhiteSpace(creatorId)
                ? Builders<Roadmap>.Filter.Empty
                : Builders<Roadmap>.Filter.Eq(r => r.CreatorId, creatorId);

            var roadmaps = await _context.Roadmaps
                .Find(filter)
                .SortByDescending(r => r.CreatedAt)
                .ToListAsync();
            
            var response = roadmaps.Select(r => new RoadmapSummaryDto
            {
                Id = r.Id!,
                Title = r.Title,
                Engine = r.Engine,
                Description = r.Description,
                CreatorId = r.CreatorId,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            }).ToList();

            return Ok(response);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoadmapResponseDto>> GetRoadmap(string id)
        {
            var roadmap = await _context.Roadmaps.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (roadmap == null) return NotFound("Không tìm thấy Roadmap!");

            return Ok(await MapRoadmapResponseAsync(roadmap));
        }

        [Authorize]
        [HttpPost]
        public async Task<ActionResult<RoadmapResponseDto>> CreateRoadmap([FromBody] SaveRoadmapRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Tiêu đề roadmap không được để trống.");
            }

            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var creatorId = !string.IsNullOrWhiteSpace(userIdFromToken) ? userIdFromToken : request.CreatorId;

            var roadmap = new Roadmap
            {
                Title = request.Title.Trim(),
                Engine = "Custom",
                Description = "Roadmap được tạo từ Roadmap Builder",
                Difficulty = "All Levels",
                CreatorId = creatorId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                NodesLayout = new List<NodeLayout>()
            };

            var persisted = await PersistBuilderNodesAsync(request);
            roadmap.NodesLayout = persisted.NodesLayout;

            await _context.Roadmaps.InsertOneAsync(roadmap);

            var response = await MapRoadmapResponseAsync(roadmap);
            return CreatedAtAction(nameof(GetRoadmap), new { id = roadmap.Id }, response);
        }

        [Authorize]
        [HttpPut("{id}")]
        public async Task<ActionResult<RoadmapResponseDto>> UpdateRoadmap(string id, [FromBody] SaveRoadmapRequestDto request)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest("Tiêu đề roadmap không được để trống.");
            }

            var roadmap = await _context.Roadmaps.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (roadmap == null)
            {
                return NotFound("Không tìm thấy Roadmap!");
            }

            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(roadmap.CreatorId) && !string.IsNullOrWhiteSpace(userIdFromToken) && roadmap.CreatorId != userIdFromToken)
            {
                return Forbid();
            }

            roadmap.Title = request.Title.Trim();
            roadmap.CreatorId = !string.IsNullOrWhiteSpace(roadmap.CreatorId) ? roadmap.CreatorId : request.CreatorId;
            roadmap.UpdatedAt = DateTime.UtcNow;
            roadmap.Difficulty = string.IsNullOrWhiteSpace(roadmap.Difficulty) ? "All Levels" : roadmap.Difficulty;

            var persisted = await PersistBuilderNodesAsync(request);
            roadmap.NodesLayout = persisted.NodesLayout;

            await _context.Roadmaps.ReplaceOneAsync(r => r.Id == id, roadmap);

            return Ok(await MapRoadmapResponseAsync(roadmap));
        }

        [Authorize]
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteRoadmap(string id)
        {
            var roadmap = await _context.Roadmaps.Find(r => r.Id == id).FirstOrDefaultAsync();
            if (roadmap == null)
            {
                return NotFound("Không tìm thấy Roadmap!");
            }

            var userIdFromToken = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (!string.IsNullOrWhiteSpace(roadmap.CreatorId) && !string.IsNullOrWhiteSpace(userIdFromToken) && roadmap.CreatorId != userIdFromToken)
            {
                return Forbid();
            }

            await _context.Roadmaps.DeleteOneAsync(r => r.Id == id);
            return Ok(new { message = "Xóa roadmap thành công.", id });
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

        private async Task<RoadmapResponseDto> MapRoadmapResponseAsync(Roadmap roadmap)
        {
            var nodeIds = roadmap.NodesLayout.Select(nl => nl.NodeId).ToList();
            var nodesData = await _context.Nodes.Find(n => nodeIds.Contains(n.Id)).ToListAsync();

            return new RoadmapResponseDto
            {
                Id = roadmap.Id!,
                Title = roadmap.Title,
                CreatorId = roadmap.CreatorId,
                CreatedAt = roadmap.CreatedAt,
                UpdatedAt = roadmap.UpdatedAt,
                Nodes = nodesData.Select(n => new FlowNodeDto
                {
                    Id = n.Id!,
                    Type = string.IsNullOrWhiteSpace(n.Category) ? "default" : n.Category,
                    Position = new FlowPosition
                    {
                        X = roadmap.NodesLayout.FirstOrDefault(l => l.NodeId == n.Id)?.X ?? 0,
                        Y = roadmap.NodesLayout.FirstOrDefault(l => l.NodeId == n.Id)?.Y ?? 0
                    },
                    Data = new FlowData
                    {
                        Label = n.Name,
                        Description = n.Description,
                        Category = n.Category,
                        Resources = n.Resources,
                        Prerequisites = n.Prerequisites
                    }
                }).ToList(),
                Edges = nodesData.Where(n => n.ParentId != null && nodeIds.Contains(n.ParentId)).Select(n => new FlowEdgeDto
                {
                    Id = $"e-{n.ParentId}-{n.Id}",
                    Source = n.ParentId!,
                    Target = n.Id!
                }).ToList()
            };
        }

        private async Task<(List<NodeLayout> NodesLayout, Dictionary<string, string> NodeIdMap)> PersistBuilderNodesAsync(SaveRoadmapRequestDto request)
        {
            var nodeIdMap = new Dictionary<string, string>();
            var persistedNodes = new Dictionary<string, Node>();
            var nodesLayout = new List<NodeLayout>();

            foreach (var builderNode in request.Nodes)
            {
                Node? node = null;

                if (!string.IsNullOrWhiteSpace(builderNode.Id) && ObjectId.TryParse(builderNode.Id, out _))
                {
                    node = await _context.Nodes.Find(n => n.Id == builderNode.Id).FirstOrDefaultAsync();
                }

                if (node == null)
                {
                    node = new Node();
                }

                node.Name = string.IsNullOrWhiteSpace(builderNode.Content) ? "Untitled Node" : builderNode.Content;
                node.Engine = "Custom";
                node.Category = string.IsNullOrWhiteSpace(builderNode.Type) ? "default" : builderNode.Type;
                node.Description = builderNode.Link ?? string.Empty;
                node.Resources = new List<string>();

                if (string.IsNullOrWhiteSpace(node.Id))
                {
                    await _context.Nodes.InsertOneAsync(node);
                }
                else
                {
                    await _context.Nodes.ReplaceOneAsync(n => n.Id == node.Id, node);
                }

                nodeIdMap[builderNode.Id] = node.Id!;
                persistedNodes[node.Id!] = node;

                nodesLayout.Add(new NodeLayout
                {
                    NodeId = node.Id!,
                    X = builderNode.X,
                    Y = builderNode.Y
                });
            }

            var incomingParentMap = new Dictionary<string, string>();
            foreach (var connection in request.Connections)
            {
                if (!nodeIdMap.TryGetValue(connection.FromNodeId, out var sourceId) ||
                    !nodeIdMap.TryGetValue(connection.ToNodeId, out var targetId) ||
                    sourceId == targetId)
                {
                    continue;
                }

                if (!incomingParentMap.ContainsKey(targetId))
                {
                    incomingParentMap[targetId] = sourceId;
                }
            }

            foreach (var kvp in persistedNodes)
            {
                var nodeId = kvp.Key;
                var node = kvp.Value;

                if (incomingParentMap.TryGetValue(nodeId, out var parentId))
                {
                    node.ParentId = parentId;
                    node.Prerequisites = new List<string> { parentId };
                }
                else
                {
                    node.ParentId = null;
                    node.Prerequisites = new List<string>();
                }

                await _context.Nodes.ReplaceOneAsync(n => n.Id == nodeId, node);
            }

            return (nodesLayout, nodeIdMap);
        }
    }
}