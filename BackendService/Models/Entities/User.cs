using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    [BsonIgnoreExtraElements]
    public class User
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("user_name")]
        public string UserName { get; set; } = string.Empty;

        [BsonElement("password")]
        public string Password { get; set; } = null!;

        [BsonElement("full_name")]
        public string? FullName { get; set; }

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("role")]
        public int Role { get; set; } = 1;

        [BsonElement("bio")]
        public string bio { get; set; } = null!;
        [BsonElement("avatar_url")]
        public string? avatar { get; set; } = null;
        [BsonElement("cover_url")]
        public string? CoverUrl { get; set; } = null;
        [BsonElement("create_at")]
        public DateTime? CreateAt { get; set; } = null;

        [BsonElement("phone")]
        public string? Phone { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("birth_date")]
        public string? BirthDate { get; set; }

        [BsonElement("links")]
        public UserLinks Links { get; set; } = new();

        [BsonElement("skills")]
        public List<string> Skills { get; set; } = new();

        [BsonElement("completed_nodes")]
        public List<string> CompletedNodes { get; set; } = new();
        [BsonElement("interested_nodes")]
        public List<string> InterestedNodes { get; set; } = new();

        [BsonElement("onboarding_responses")]
        public Dictionary<string, string> OnboardingResponses { get; set; } = new();

        [BsonElement("is_approved")]
        public bool IsApproved { get; set; } = false;
    }

    [BsonIgnoreExtraElements]
    public class UserLinks
    {
        [BsonElement("github")]
        public string? Github { get; set; }

        [BsonElement("portfolio")]
        public string? Portfolio { get; set; }

        [BsonElement("linkedin")]
        public string? LinkedIn { get; set; }

        [BsonElement("facebook")]
        public string? Facebook { get; set; }
    }
}