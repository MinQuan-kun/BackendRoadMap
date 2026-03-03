using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    public class QuestionBank
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("question_text")]
        public string QuestionText { get; set; } = null!;

        [BsonElement("options")]
        public List<QuestionOption> Options { get; set; } = new();
    }

    public class QuestionOption
    {
        [BsonElement("text")]
        public string Text { get; set; } = null!;

        [BsonElement("mapping_nodes")]
        public List<string> MappingNodes { get; set; } = new(); // Các Node ID liên quan đến câu trả lời
    }
}