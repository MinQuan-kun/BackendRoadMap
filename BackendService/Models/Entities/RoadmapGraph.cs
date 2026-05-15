using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class RoadmapGraph
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("graph_type")]
        public string GraphType { get; set; } = "official";

        [BsonElement("owner_user_id")]
        public string? OwnerUserId { get; set; }

        [BsonElement("node_ids")]
        public List<string> NodeIds { get; set; } = new();

        [BsonElement("edge_ids")]
        public List<string> EdgeIds { get; set; } = new();

        [BsonElement("layout_type")]
        public string LayoutType { get; set; } = "dagre";

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}