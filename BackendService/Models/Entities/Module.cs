using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class Module
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("course_id")]
        public string CourseId { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("lesson_ids")]
        public List<string> LessonIds { get; set; } = new();

        [BsonElement("order")]
        public int Order { get; set; }
    }
}
