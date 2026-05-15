using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class RoadmapNode
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("graph_id")]
        public string GraphId { get; set; } = string.Empty;

        [BsonElement("node_type")]
        public string NodeType { get; set; } = "course";

        [BsonElement("reference_id")]
        public string ReferenceId { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("position_x")]
        public double PositionX { get; set; }

        [BsonElement("position_y")]
        public double PositionY { get; set; }
    }
}