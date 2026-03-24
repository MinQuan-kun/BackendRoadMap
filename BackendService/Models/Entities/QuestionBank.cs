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

        [BsonElement("parent_question_id")]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? ParentQuestionId { get; set; }

        [BsonElement("required_option_text")]
        public string? RequiredOptionText { get; set; }

        [BsonElement("options")]
        public List<QuestionOption> Options { get; set; } = new();
    }

    public class QuestionOption
    {
        [BsonElement("text")]
        public string Text { get; set; } = null!;

        [BsonElement("mapping_nodes")]
        public List<string> MappingNodes { get; set; } = new();
    }
}