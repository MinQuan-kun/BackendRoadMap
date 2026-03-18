using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    public class Roadmap
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = null!;

        [BsonElement("creator_id")]
        public string? CreatorId { get; set; }

        [BsonElement("nodes")]
        public List<RoadmapNode> Nodes { get; set; } = new();

        [BsonElement("connections")]
        public List<RoadmapConnection> Connections { get; set; } = new();

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    }

    public class RoadmapNode
    {
        [BsonElement("id")]
        public string Id { get; set; } = null!;

        [BsonElement("type")]
        public string Type { get; set; } = null!;

        [BsonElement("content")]
        public string? Content { get; set; }

        [BsonElement("x")]
        public double X { get; set; }

        [BsonElement("y")]
        public double Y { get; set; }

        [BsonElement("width")]
        public double Width { get; set; }

        [BsonElement("height")]
        public double Height { get; set; }

        [BsonElement("link")]
        public string? Link { get; set; }

        [BsonElement("style")]
        public BsonDocument Style { get; set; } = new();

        [BsonElement("created_at")]
        public long? CreatedAt { get; set; }

        [BsonElement("updated_at")]
        public long? UpdatedAt { get; set; }
    }

    public class RoadmapConnection
    {
        [BsonElement("id")]
        public string Id { get; set; } = null!;

        [BsonElement("from_node_id")]
        public string FromNodeId { get; set; } = null!;

        [BsonElement("to_node_id")]
        public string ToNodeId { get; set; } = null!;

        [BsonElement("from_point")]
        public string FromPoint { get; set; } = "bottom";

        [BsonElement("to_point")]
        public string ToPoint { get; set; } = "top";
    }
}