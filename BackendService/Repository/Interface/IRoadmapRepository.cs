using BackendService.Models.Entities;

namespace BackendService.Repository.Interface
{
    public interface IRoadmapRepository
    {
        Task CreateGraphAsync(RoadmapGraph graph, CancellationToken cancellationToken = default);
        Task UpdateGraphAsync(string id, RoadmapGraph graph, CancellationToken cancellationToken = default);
        Task CreateNodesAsync(List<RoadmapNode> nodes, CancellationToken cancellationToken = default);
        Task CreateEdgesAsync(List<RoadmapEdge> edges, CancellationToken cancellationToken = default);
        Task DeleteNodesAndEdgesByGraphIdAsync(string graphId, CancellationToken cancellationToken = default);
    }
}
