namespace BackendService.Models.DTOs;
public class RoadmapResponseDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string? CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public List<FlowNodeDto> Nodes { get; set; } = new();
    public List<FlowEdgeDto> Edges { get; set; } = new();
}

public class FlowNodeDto
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = "default";
    public FlowPosition Position { get; set; } = null!;
    public FlowData Data { get; set; } = null!;
}

public class FlowEdgeDto
{
    public string Id { get; set; } = null!;
    public string Source { get; set; } = null!;
    public string Target { get; set; } = null!;
    public string Type { get; set; } = "true";
    public string? FromPoint { get; set; }
    public string? ToPoint { get; set; }
}

public class FlowPosition
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class FlowData
{
    public string Label { get; set; } = null!;
    public string? Description { get; set; }
    public string? Category { get; set; }
    public List<string> Resources { get; set; } = new();
    public List<string> Prerequisites { get; set; } = new();
}

public class RoadmapSummaryDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
    public string Engine { get; set; } = null!;
    public string? Description { get; set; }
    public string? CreatorId { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

public class SaveRoadmapRequestDto
{
    public string Title { get; set; } = null!;
    public string? CreatorId { get; set; }
    public List<BuilderNodeDto> Nodes { get; set; } = new();
    public List<BuilderConnectionDto> Connections { get; set; } = new();
}

public class BuilderNodeDto
{
    public string Id { get; set; } = null!;
    public string Type { get; set; } = "default";
    public string Content { get; set; } = string.Empty;
    public double X { get; set; }
    public double Y { get; set; }
    public double Width { get; set; }
    public double Height { get; set; }
    public string? Link { get; set; }
    public Dictionary<string, object>? Style { get; set; }
    public long? CreatedAt { get; set; }
    public long? UpdatedAt { get; set; }
}

public class BuilderConnectionDto
{
    public string Id { get; set; } = null!;
    public string FromNodeId { get; set; } = null!;
    public string ToNodeId { get; set; } = null!;
    public string FromPoint { get; set; } = "bottom";
    public string ToPoint { get; set; } = "top";
}
