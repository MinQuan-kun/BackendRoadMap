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

        // Lấy danh sách roadmap với các bộ lọc
        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoadmapSummaryDto>>> GetAllRoadmaps(
            [FromQuery] string? creatorId, 
            [FromQuery] string? search,
            [FromQuery] bool includeOfficial = false,
            [FromQuery] bool onlyOfficial = false)
        {
            var builder = Builders<Roadmap>.Filter;
            var filter = builder.Empty;

            if (onlyOfficial)
            {
                filter = builder.In(r => r.Engine, new[] { "Unity", "Unreal" });
            }
            else if (!string.IsNullOrWhiteSpace(creatorId))
            {
                var userFilter = builder.Eq(r => r.CreatorId, creatorId);
                if (includeOfficial)
                {
                    var officialFilter = builder.In(r => r.Engine, new[] { "Unity", "Unreal" });
                    filter = builder.Or(userFilter, officialFilter);
                }
                else
                {
                    filter = userFilter;
                }
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchFilter = builder.Regex(r => r.Title, new BsonRegularExpression(search, "i"));
                filter = builder.And(filter, searchFilter);
            }

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

        // Lấy danh sách node có sẵn trong DB (Node Library)
        [HttpGet("available-nodes")]
        public async Task<ActionResult<IEnumerable<AvailableNodeDto>>> GetAvailableNodes(
            [FromQuery] string? engine,
            [FromQuery] string? search,
            [FromQuery] string? category)
        {
            var builder = Builders<Node>.Filter;
            var filter = builder.Empty;

            if (!string.IsNullOrWhiteSpace(engine))
            {
                filter = builder.And(filter, builder.Eq(n => n.Engine, engine));
            }

            if (!string.IsNullOrWhiteSpace(search))
            {
                filter = builder.And(filter, builder.Regex(n => n.Name, new BsonRegularExpression(search, "i")));
            }

            if (!string.IsNullOrWhiteSpace(category))
            {
                filter = builder.And(filter, builder.Regex(n => n.Category, new BsonRegularExpression(category, "i")));
            }

            var nodes = await _context.Nodes.Find(filter).Limit(200).ToListAsync();

            var response = nodes.Select(n => new AvailableNodeDto
            {
                Id = n.Id!,
                Name = n.Name,
                Engine = n.Engine,
                Category = n.Category,
                Description = n.Description,
                ParentId = n.ParentId,
                Resources = n.Resources,
                HasContent = (n.ContentBlocks != null && n.ContentBlocks.Count > 0) || !string.IsNullOrWhiteSpace(n.VideoUrl),
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
                Engine = request.Engine ?? "Custom",
                Description = request.Description ?? "Roadmap được tạo từ Roadmap Builder",
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
            roadmap.Engine = request.Engine ?? roadmap.Engine;
            roadmap.Description = request.Description ?? roadmap.Description;
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
            var nodesData = await _context.Nodes.Find(n => n.Id != null && nodeIds.Contains(n.Id)).ToListAsync();
            var layoutLookup = roadmap.NodesLayout.ToDictionary(l => l.NodeId, l => l);

            return new RoadmapResponseDto
            {
                Id = roadmap.Id!,
                Title = roadmap.Title,
                Engine = roadmap.Engine,
                Description = roadmap.Description,
                CreatorId = roadmap.CreatorId,
                CreatedAt = roadmap.CreatedAt,
                UpdatedAt = roadmap.UpdatedAt,
                Nodes = nodesData.Select(n =>
                {
                    layoutLookup.TryGetValue(n.Id!, out var layout);
                    return new FlowNodeDto
                    {
                        Id = n.Id!,
                        Type = string.IsNullOrWhiteSpace(n.Category) ? "default" : n.Category,
                        Position = new FlowPosition
                        {
                            X = layout?.X ?? 0,
                            Y = layout?.Y ?? 0
                        },
                        Color = layout?.Color,
                        Style = layout?.Style,
                        Data = new FlowData
                        {
                            Label = n.Name,
                            Description = n.Description,
                            Category = n.Category,
                            Resources = n.Resources,
                            Prerequisites = n.Prerequisites,
                            ContentBlocks = n.ContentBlocks,
                            VideoUrl = n.VideoUrl
                        }
                    };
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
                    Y = builderNode.Y,
                    Color = builderNode.Color,
                    Width = builderNode.Width > 0 ? builderNode.Width : null,
                    Height = builderNode.Height > 0 ? builderNode.Height : null,
                    Style = builderNode.Style?.ToDictionary(kvp => kvp.Key, kvp => ConvertJsonElement(kvp.Value) ?? string.Empty)
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

        private object? ConvertJsonElement(object? obj)
        {
            if (obj is System.Text.Json.JsonElement element)
            {
                switch (element.ValueKind)
                {
                    case System.Text.Json.JsonValueKind.String: return element.GetString();
                    case System.Text.Json.JsonValueKind.Number:
                        if (element.TryGetInt32(out var i)) return i;
                        if (element.TryGetInt64(out var l)) return l;
                        return element.GetDouble();
                    case System.Text.Json.JsonValueKind.True: return true;
                    case System.Text.Json.JsonValueKind.False: return false;
                    case System.Text.Json.JsonValueKind.Null: return null;
                    default: return element.GetRawText();
                }
            }
            return obj;
        }
    }
}