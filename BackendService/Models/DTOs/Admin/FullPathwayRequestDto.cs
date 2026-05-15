using System.Collections.Generic;
using BackendService.Models.Entities;

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
        public GraphRequestDto? Graph { get; set; }
    }

    public class FullCourseRequestDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FullModuleRequestDto> Modules { get; set; } = new();
    }

    public class GraphRequestDto
    {
        public List<NodePositionDto> Nodes { get; set; } = new();
        public List<EdgeDto> Edges { get; set; } = new();
    }

    public class NodePositionDto
    {
        public string CourseId { get; set; } = string.Empty;
        public double X { get; set; }
        public double Y { get; set; }
    }

    public class EdgeDto
    {
        public string SourceId { get; set; } = string.Empty;
        public string TargetId { get; set; } = string.Empty;
    }

    public class FullModuleRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<FullLessonRequestDto> Lessons { get; set; } = new();
    }

    public class FullLessonRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public string Type { get; set; } = "text";
        public string? Difficulty { get; set; }
        public int XPReward { get; set; }
    }
}
