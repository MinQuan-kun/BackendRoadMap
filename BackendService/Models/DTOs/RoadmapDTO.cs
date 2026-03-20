namespace BackendService.Models.DTOs;
public class RoadmapResponseDto
{
    public string Id { get; set; } = null!;
    public string Title { get; set; } = null!;
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
}

public class FlowPosition
{
    public double X { get; set; }
    public double Y { get; set; }
}

public class FlowData
{
    public string Label { get; set; } = null!;
}
