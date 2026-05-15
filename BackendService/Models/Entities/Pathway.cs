using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Pathway
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("slug")]
        public string Slug { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("thumbnail")]
        public string? Thumbnail { get; set; }

        [BsonElement("difficulty")]
        public string Difficulty { get; set; } = "beginner";

        [BsonElement("estimated_hours")]
        public int EstimatedHours { get; set; }

        [BsonElement("tags")]
        public List<string> Tags { get; set; } = new();

        [BsonElement("course_ids")]
        public List<string> CourseIds { get; set; } = new();

        [BsonElement("roadmap_graph_id")]
        public string? RoadmapGraphId { get; set; }

        [BsonElement("is_official")]
        public bool IsOfficial { get; set; } = true;

        [BsonElement("created_by")]
        public string? CreatedBy { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}