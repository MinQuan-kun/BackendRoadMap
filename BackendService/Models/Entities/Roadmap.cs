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
        [BsonRepresentation(BsonType.ObjectId)]
        public string CreatorId { get; set; } = null!;

        [BsonElement("nodes_layout")]
        public List<NodeLayout> NodesLayout { get; set; } = new();
    }

    public class NodeLayout
    {
        [BsonElement("node_id")]
        public string NodeId { get; set; } = null!;

        [BsonElement("x")]
        public double X { get; set; } // Tọa độ X trên canvas

        [BsonElement("y")]
        public double Y { get; set; } // Tọa độ Y trên canvas
    }
}