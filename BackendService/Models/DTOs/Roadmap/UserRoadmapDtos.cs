using System.Collections.Generic;

namespace BackendService.Models.DTOs.Roadmap
{
    public class UserRoadmapRequestDto
    {
        public string Title { get; set; } = string.Empty;
        public List<UserNodeDto> Nodes { get; set; } = new();
        public List<UserEdgeDto> Edges { get; set; } = new();
    }

    public class UserNodeDto
    {
        public string Id { get; set; } = string.Empty;
        public string Title { get; set; } = string.Empty;
        public string NodeType { get; set; } = "default";
        public double PositionX { get; set; }
        public double PositionY { get; set; }
        public string? ReferenceId { get; set; }
    }

    public class UserEdgeDto
    {
        public string SourceNodeId { get; set; } = string.Empty;
        public string TargetNodeId { get; set; } = string.Empty;
    }
}
