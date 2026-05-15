using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Lesson
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("module_id")]
        public string ModuleId { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("video_url")]
        public string? VideoUrl { get; set; }

        [BsonElement("difficulty")]
        public string Difficulty { get; set; } = "easy";

        [BsonElement("estimated_minutes")]
        public int EstimatedMinutes { get; set; }

        [BsonElement("task_ids")]
        public List<string> TaskIds { get; set; } = new();

        [BsonElement("content_blocks")]
        public List<ContentBlock> ContentBlocks { get; set; } = new();

        [BsonElement("resources")]
        public List<string> Resources { get; set; } = new();

        [BsonElement("prerequisites")]
        public List<string> Prerequisites { get; set; } = new();

        [BsonElement("quiz_ids")]
        public List<string> QuizIds { get; set; } = new();

        [BsonElement("xp_reward")]
        public int XPReward { get; set; }
    }

    public class ContentBlock
    {
        [BsonElement("type")]
        public string Type { get; set; } = "text";

        [BsonElement("content")]
        public string Content { get; set; } = string.Empty;

        [BsonElement("language")]
        public string? Language { get; set; }

        [BsonElement("caption")]
        public string? Caption { get; set; }
    }
}