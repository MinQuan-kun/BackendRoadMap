using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class RoadmapEdge
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("graph_id")]
        public string GraphId { get; set; } = string.Empty;

        [BsonElement("source_node_id")]
        public string SourceNodeId { get; set; } = string.Empty;

        [BsonElement("target_node_id")]
        public string TargetNodeId { get; set; } = string.Empty;
    }
}