using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    public class Application
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string Id { get; set; } = string.Empty;

        [BsonElement("job_id")]
        public string JobId { get; set; } = null!;

        [BsonElement("user_id")]
        public string UserId { get; set; } = null!;

        [BsonElement("matching_score")]
        public double MatchingScore { get; set; }

        [BsonElement("status")]
        public string Status { get; set; } = "Pending"; // Pending, Accepted, Rejected

        [BsonElement("applied_at")]
        public DateTime AppliedAt { get; set; } = DateTime.UtcNow;

        public User? User { get; set; }
        public Job? Job { get; set; }   
    }
}