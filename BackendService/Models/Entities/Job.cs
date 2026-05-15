using MongoDB.Bson;
using MongoDB.Bson.Serialization.Attributes;

namespace BackendService.Models.Entities.Recruitment
{
    [BsonIgnoreExtraElements]
    public class Job
    {
        [BsonId]
        [BsonRepresentation(BsonType.ObjectId)]
        public string? Id { get; set; }

        [BsonElement("recruiter_id")]
        public string RecruiterId { get; set; } = string.Empty;

        [BsonElement("title")]
        public string Title { get; set; } = string.Empty;

        [BsonElement("description")]
        public string? Description { get; set; }

        [BsonElement("company_name")]
        public string CompanyName { get; set; } = string.Empty;

        [BsonElement("location")]
        public string? Location { get; set; }

        [BsonElement("job_type")]
        public string JobType { get; set; } = "remote";

        [BsonElement("salary")]
        public string? Salary { get; set; }

        [BsonElement("experience_level")]
        public string? ExperienceLevel { get; set; }

        [BsonElement("required_skill_tags")]
        public List<string> RequiredSkillTags { get; set; } = new();

        [BsonElement("required_course_ids")]
        public List<string> RequiredCourseIds { get; set; } = new();

        [BsonElement("roadmap_graph_id")]
        public string? RoadmapGraphId { get; set; }

        [BsonElement("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}