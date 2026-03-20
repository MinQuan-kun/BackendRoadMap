using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    public class Node
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("name")]
        public string Name { get; set; } = null!;
        [BsonElement("engine")]
        public string? Engine { get; set; } = null;

        [BsonElement("parent_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ParentId { get; set; }

        [BsonElement("category")]
        public string Category { get; set; } = null!;

        [BsonElement("description")]
        public string Description { get; set; } = null!;

        [BsonElement("resources")]
        public List<string> Resources { get; set; } = new();

        [BsonElement("prerequisites")]
        public List<string> Prerequisites { get; set; } = new();
    }
}