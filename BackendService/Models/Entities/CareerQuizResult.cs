using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class CareerQuizResult
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_id")]
        public string UserId { get; set; } = string.Empty;

        [BsonElement("quiz_id")]
        public string QuizId { get; set; } = string.Empty;

        [BsonElement("answers")]
        public Dictionary<string, string> Answers { get; set; } = new();

        [BsonElement("recommended_pathway_ids")]
        public List<string> RecommendedPathwayIds { get; set; } = new();

        [BsonElement("recommended_course_ids")]
        public List<string> RecommendedCourseIds { get; set; } = new();

        [BsonElement("explicit_preference_pathway_id")]
        public string? ExplicitPreferencePathwayId { get; set; }

        [BsonElement("has_conflict")]
        public bool HasConflict { get; set; }

        [BsonElement("generated_roadmap_graph_id")]
        public string? GeneratedRoadmapGraphId { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}