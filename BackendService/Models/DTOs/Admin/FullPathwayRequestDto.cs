using System.Collections.Generic;

namespace BackendService.Models.DTOs.Admin
{
    public class FullPathwayRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public string Thumbnail { get; set; } = string.Empty;
        public string Difficulty { get; set; } = "beginner";
        public int EstimatedHours { get; set; }
        public List<string> Tags { get; set; } = new();
        public bool IsOfficial { get; set; }
        public List<FullCourseRequestDto> Courses { get; set; } = new();
    }

    public class FullCourseRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FullModuleRequestDto> Modules { get; set; } = new();
    }

    public class FullModuleRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<string> Lessons { get; set; } = new();
    }
}
