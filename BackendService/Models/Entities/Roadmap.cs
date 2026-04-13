using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements] // Giúp code không bị crash khi gặp trường lạ trong DB
    public class Roadmap
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = null!;

        [BsonElement("engine")]
        public string Engine { get; set; } = null!;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("difficulty")]
        public string Difficulty { get; set; } = "All Levels";

        [BsonElement("creator_id")]
        public string? CreatorId { get; set; }

        [BsonElement("nodes_layout")]
        public List<NodeLayout> NodesLayout { get; set; } = new();

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; }

        [BsonElement("updated_at")]
        public DateTime? UpdatedAt { get; set; }
    }

    public class NodeLayout
    {
        [BsonElement("node_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string NodeId { get; set; } = null!;

        [BsonElement("x")]
        public double X { get; set; }

        [BsonElement("y")]
        public double Y { get; set; }
    }
}