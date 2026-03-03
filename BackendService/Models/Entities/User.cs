using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_name")]
        public string UserName { get; set; } = null!;

        [BsonElement("password")]
        public string Password { get; set; } = null!;

        [BsonElement("full_name")]
        public string FullName { get; set; } = null!;

        [BsonElement("email")]
        public string Email { get; set; } = null!;

        [BsonElement("role")]
        public int Role { get; set; } = 1;

        [BsonElement("completed_nodes")]
        public List<string> CompletedNodes { get; set; } = new();

        [BsonElement("onboarding_responses")]
        public Dictionary<string, string> OnboardingResponses { get; set; } = new();
    }
}