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

        // =========================
        // AUTH
        // =========================

        [BsonElement("user_name")]
        public string UserName { get; set; } = string.Empty;

        [BsonElement("email")]
        public string Email { get; set; } = string.Empty;

        [BsonElement("password_hash")]
        public string PasswordHash { get; set; } = string.Empty;

        [BsonElement("role")]
        public UserRole Role { get; set; } = UserRole.User;

        [BsonElement("status")]
        public UserStatus Status { get; set; } = UserStatus.Active;

        // =========================
        // PROFILE
        // =========================

        [BsonElement("display_name")]
        public string? DisplayName { get; set; }

        [BsonElement("bio")]
        public string? Bio { get; set; }

        [BsonElement("avatar_url")]
        public string? AvatarUrl { get; set; }

        [BsonElement("cover_url")]
        public string? CoverUrl { get; set; }

        [BsonElement("phone")]
        public string? Phone { get; set; }

        [BsonElement("address")]
        public string? Address { get; set; }

        [BsonElement("birth_date")]
        public string? BirthDate { get; set; }

        // =========================
        // SOCIAL LINKS
        // =========================

        [BsonElement("links")]
        public UserLinks Links { get; set; } = new();

        // =========================
        // SKILLS
        // =========================

        [BsonElement("skill_tags")]
        public List<string> SkillTags { get; set; } = new();

        // =========================
        // SYSTEM
        // =========================

        [BsonElement("is_recruiter_verified")]
        public bool IsRecruiterVerified { get; set; } = false;

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [BsonElement("updated_at")]
        public DateTime? UpdatedAt { get; set; }
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

    public enum UserRole
    {
        Admin = 0,
        User = 1,
        Recruiter = 2
    }

    public enum UserStatus
    {
        Active = 1,
        Suspended = 2,
        Banned = 3
    }
}