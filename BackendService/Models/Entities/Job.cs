using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    public class Job
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("company_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string CompanyId { get; set; } = null!;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string Description { get; set; } = string.Empty;

        [BsonElement("location")]
        public string Location { get; set; } = string.Empty;

        [BsonElement("salary")]
        public string Salary { get; set; } = string.Empty;

        [BsonElement("skills")]
        public List<string> Skills { get; set; } = new();

        [BsonElement("experience_level")]
        public string ExperienceLevel { get; set; } = string.Empty;

        [BsonElement("target_roadmap_id")]
        public string TargetRoadmapId { get; set; } = null!;

        [BsonElement("matching_rate")]
        public double MatchingRate { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public Company? Company { get; set; }
    }
}