using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Course
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("pathway_id")]
        public string? PathwayId { get; set; }

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("thumbnail")]
        public string? Thumbnail { get; set; }

        [BsonElement("cover_url")]
        public string? CoverUrl { get; set; }

        [BsonElement("difficulty")]
        public string Difficulty { get; set; } = "beginner";

        [BsonElement("estimated_hours")]
        public int EstimatedHours { get; set; }

        [BsonElement("module_ids")]
        public List<string> ModuleIds { get; set; } = new();

        [BsonElement("skill_tags")]
        public List<string> SkillTags { get; set; } = new();

        [BsonElement("xp_reward")]
        public int XPReward { get; set; }

        [BsonElement("order")]
        public int Order { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}