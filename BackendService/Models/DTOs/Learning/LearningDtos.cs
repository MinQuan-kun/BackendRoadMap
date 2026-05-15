namespace BackendService.Models.DTOs.Learning
{
    public class PathwayDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Thumbnail { get; set; }
        public string Difficulty { get; set; } = "beginner";
        public int EstimatedHours { get; set; }
        public List<string> Tags { get; set; } = new();
        public List<string> CourseIds { get; set; } = new();
        public string? RoadmapGraphId { get; set; }
        public bool IsOfficial { get; set; }
    }

    public class CourseDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? Thumbnail { get; set; }
        public string Difficulty { get; set; } = "beginner";
        public int EstimatedHours { get; set; }
        public List<string> ModuleIds { get; set; } = new();
        public List<string> SkillTags { get; set; } = new();
        public int XPReward { get; set; }
        public int Order { get; set; }
    }

    public class ModuleDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<string> LessonIds { get; set; } = new();
        public int Order { get; set; }
    }

    public class LessonDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public string? VideoUrl { get; set; }
        public string Difficulty { get; set; } = "easy";
        public int EstimatedMinutes { get; set; }
        public List<string> TaskIds { get; set; } = new();
        public List<string> QuizIds { get; set; } = new();
        public int XPReward { get; set; }
    }
}
