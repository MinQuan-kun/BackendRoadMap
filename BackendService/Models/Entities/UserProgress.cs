using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class UserProgress
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_id")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("active_roadmap_id")]
        public string? ActiveRoadmapId { get; set; }

        [BsonElement("completed_course_ids")]
        public List<string> CompletedCourseIds { get; set; } = new();

        [BsonElement("completed_lesson_ids")]
        public List<string> CompletedLessonIds { get; set; } = new();

        [BsonElement("completed_task_ids")]
        public List<string> CompletedTaskIds { get; set; } = new();

        [BsonElement("followed_roadmap_ids")]
        public List<string> FollowedRoadmapIds { get; set; } = new();

        [BsonElement("xp")]
        public int XP { get; set; }

        [BsonElement("level")]
        public int Level { get; set; } = 1;

        [BsonElement("last_activity")]
        public DateTime? LastActivity { get; set; }
    }
}