using BackendService.Data;
using BackendService.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Bson;
using MongoDB.Driver;
using System.Text.Json;

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

        [HttpGet]
        public async Task<ActionResult<IEnumerable<RoadmapResponseDto>>> GetRoadmaps(
            [FromQuery] string? creatorId,
            CancellationToken cancellationToken)
        {
            FilterDefinition<Roadmap> filter = Builders<Roadmap>.Filter.Empty;
            if (!string.IsNullOrWhiteSpace(creatorId))
            {
                filter = Builders<Roadmap>.Filter.Eq(r => r.CreatorId, creatorId);
            }

            var roadmaps = await _context.Roadmaps
                .Find(filter)
                .SortByDescending(r => r.UpdatedAt)
                .ToListAsync(cancellationToken);

            return Ok(roadmaps.Select(MapToResponse));
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<RoadmapResponseDto>> GetRoadmapById(string id, CancellationToken cancellationToken)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest(new { message = "Invalid roadmap id format." });
            }

            var roadmap = await _context.Roadmaps
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (roadmap is null)
            {
                return NotFound(new { message = "Roadmap not found." });
            }

            return Ok(MapToResponse(roadmap));
        }

        [HttpPost]
        public async Task<ActionResult<RoadmapResponseDto>> CreateRoadmap(
            [FromBody] CreateRoadmapRequest request,
            CancellationToken cancellationToken)
        {
            if (string.IsNullOrWhiteSpace(request.Title))
            {
                return BadRequest(new { message = "Title is required." });
            }

            var roadmap = new Roadmap
            {
                Title = request.Title.Trim(),
                CreatorId = string.IsNullOrWhiteSpace(request.CreatorId) ? null : request.CreatorId.Trim(),
                Nodes = request.Nodes.Select(MapNode).ToList(),
                Connections = request.Connections.Select(MapConnection).ToList(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            await _context.Roadmaps.InsertOneAsync(roadmap, cancellationToken: cancellationToken);

            return CreatedAtAction(
                nameof(GetRoadmapById),
                new { id = roadmap.Id },
                MapToResponse(roadmap));
        }

        [HttpPut("{id}")]
        public async Task<ActionResult<RoadmapResponseDto>> UpdateRoadmap(
            string id,
            [FromBody] UpdateRoadmapRequest request,
            CancellationToken cancellationToken)
        {
            if (!ObjectId.TryParse(id, out _))
            {
                return BadRequest(new { message = "Invalid roadmap id format." });
            }

            var roadmap = await _context.Roadmaps
                .Find(r => r.Id == id)
                .FirstOrDefaultAsync(cancellationToken);

            if (roadmap is null)
            {
                return NotFound(new { message = "Roadmap not found." });
            }

            if (!string.IsNullOrWhiteSpace(request.Title))
            {
                roadmap.Title = request.Title.Trim();
            }

            if (request.CreatorId is not null)
            {
                roadmap.CreatorId = string.IsNullOrWhiteSpace(request.CreatorId)
                    ? null
                    : request.CreatorId.Trim();
            }

            if (request.Nodes is not null)
            {
                roadmap.Nodes = request.Nodes.Select(MapNode).ToList();
            }

            if (request.Connections is not null)
            {
                roadmap.Connections = request.Connections.Select(MapConnection).ToList();
            }

            roadmap.UpdatedAt = DateTime.UtcNow;

            await _context.Roadmaps.ReplaceOneAsync(
                r => r.Id == id,
                roadmap,
                cancellationToken: cancellationToken);

            return Ok(MapToResponse(roadmap));
        }

        private static RoadmapResponseDto MapToResponse(Roadmap roadmap)
        {
            return new RoadmapResponseDto
            {
                Id = roadmap.Id,
                Title = roadmap.Title,
                CreatorId = roadmap.CreatorId,
                Nodes = roadmap.Nodes.Select(n => new RoadmapNodeDto
                {
                    Id = n.Id,
                    Type = n.Type,
                    Content = n.Content,
                    X = n.X,
                    Y = n.Y,
                    Width = n.Width,
                    Height = n.Height,
                    Link = n.Link,
                    Style = n.Style?.ToDictionary(k => k.Name, v => BsonTypeMapper.MapToDotNetValue(v.Value))
                        ?? new Dictionary<string, object>(),
                    CreatedAt = n.CreatedAt,
                    UpdatedAt = n.UpdatedAt
                }).ToList(),
                Connections = roadmap.Connections.Select(c => new RoadmapConnectionDto
                {
                    Id = c.Id,
                    FromNodeId = c.FromNodeId,
                    ToNodeId = c.ToNodeId,
                    FromPoint = c.FromPoint,
                    ToPoint = c.ToPoint
                }).ToList(),
                CreatedAt = roadmap.CreatedAt,
                UpdatedAt = roadmap.UpdatedAt
            };
        }

        private static RoadmapNode MapNode(RoadmapNodeDto node)
        {
            return new RoadmapNode
            {
                Id = node.Id,
                Type = node.Type,
                Content = node.Content,
                X = node.X,
                Y = node.Y,
                Width = node.Width,
                Height = node.Height,
                Link = node.Link,
                Style = node.Style is null
                    ? new BsonDocument()
                    : new BsonDocument(node.Style.Select(pair => new BsonElement(pair.Key, ToBsonValue(pair.Value)))),
                CreatedAt = node.CreatedAt,
                UpdatedAt = node.UpdatedAt
            };
        }

        private static BsonValue ToBsonValue(object? value)
        {
            if (value is null)
            {
                return BsonNull.Value;
            }

            if (value is JsonElement json)
            {
                switch (json.ValueKind)
                {
                    case JsonValueKind.Object:
                    {
                        var doc = new BsonDocument();
                        foreach (var prop in json.EnumerateObject())
                        {
                            doc[prop.Name] = ToBsonValue(prop.Value);
                        }
                        return doc;
                    }
                    case JsonValueKind.Array:
                    {
                        var arr = new BsonArray();
                        foreach (var item in json.EnumerateArray())
                        {
                            arr.Add(ToBsonValue(item));
                        }
                        return arr;
                    }
                    case JsonValueKind.String:
                        return json.GetString() ?? string.Empty;
                    case JsonValueKind.Number:
                        if (json.TryGetInt64(out var intValue))
                        {
                            return intValue;
                        }
                        return json.GetDouble();
                    case JsonValueKind.True:
                        return true;
                    case JsonValueKind.False:
                        return false;
                    case JsonValueKind.Null:
                    case JsonValueKind.Undefined:
                        return BsonNull.Value;
                }
            }

            return BsonValue.Create(value);
        }

        private static RoadmapConnection MapConnection(RoadmapConnectionDto connection)
        {
            return new RoadmapConnection
            {
                Id = connection.Id,
                FromNodeId = connection.FromNodeId,
                ToNodeId = connection.ToNodeId,
                FromPoint = string.IsNullOrWhiteSpace(connection.FromPoint) ? "bottom" : connection.FromPoint,
                ToPoint = string.IsNullOrWhiteSpace(connection.ToPoint) ? "top" : connection.ToPoint
            };
        }
    }

    public class CreateRoadmapRequest
    {
        public string Title { get; set; } = string.Empty;
        public string? CreatorId { get; set; }
        public List<RoadmapNodeDto> Nodes { get; set; } = new();
        public List<RoadmapConnectionDto> Connections { get; set; } = new();
    }

    public class UpdateRoadmapRequest
    {
        public string? Title { get; set; }
        public string? CreatorId { get; set; }
        public List<RoadmapNodeDto>? Nodes { get; set; }
        public List<RoadmapConnectionDto>? Connections { get; set; }
    }

    public class RoadmapResponseDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? CreatorId { get; set; }
        public List<RoadmapNodeDto> Nodes { get; set; } = new();
        public List<RoadmapConnectionDto> Connections { get; set; } = new();
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class RoadmapNodeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public string? Content { get; set; }
        public double X { get; set; }
        public double Y { get; set; }
        public double Width { get; set; }
        public double Height { get; set; }
        public string? Link { get; set; }
        public Dictionary<string, object>? Style { get; set; }
        public long? CreatedAt { get; set; }
        public long? UpdatedAt { get; set; }
    }

    public class RoadmapConnectionDto
    {
        public string Id { get; set; } = string.Empty;
        public string FromNodeId { get; set; } = string.Empty;
        public string ToNodeId { get; set; } = string.Empty;
        public string FromPoint { get; set; } = "bottom";
        public string ToPoint { get; set; } = "top";
    }
}
