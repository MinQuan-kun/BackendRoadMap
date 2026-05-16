using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class LearningTask
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("lesson_id")]
        public string LessonId { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("task_type")]
        public string TaskType { get; set; } = "practice";

        [BsonElement("xp_reward")]
        public int XPReward { get; set; }

        [BsonElement("difficulty")]
        public string Difficulty { get; set; } = "easy";

        [BsonElement("media_url")]
        public string? MediaUrl { get; set; }

        [BsonElement("media_type")]
        public string? MediaType { get; set; }
    }
}