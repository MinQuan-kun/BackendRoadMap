using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class CareerQuestion
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("question")]
        public string Question { get; set; } = string.Empty;

        [BsonElement("options")]
        public List<QuestionOption> Options { get; set; } = new();
    }

    public class QuestionOption
    {
        [BsonElement("text")]
        public string Text { get; set; } = string.Empty;

        [BsonElement("mapping_pathway_ids")]
        public List<string> MappingPathwayIds { get; set; } = new();

        [BsonElement("mapping_course_ids")]
        public List<string> MappingCourseIds { get; set; } = new();

        [BsonElement("weight")]
        public int Weight { get; set; }
    }
}