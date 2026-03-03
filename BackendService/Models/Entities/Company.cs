using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities
{
    public class Company
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("company_name")]
        public string CompanyName { get; set; } = null!;

        [BsonElement("logo_url")]
        public string? LogoUrl { get; set; }

        [BsonElement("website_url")]
        public string? WebsiteUrl { get; set; }

        [BsonElement("admin_ids")]
        public List<string> AdminIds { get; set; } = new(); // Danh sách User_id quản lý công ty
    }
}