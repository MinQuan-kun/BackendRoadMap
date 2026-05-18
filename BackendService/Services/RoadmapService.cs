using BackendService.Models.DTOs.Roadmap;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using BackendService.Services.Interface;

namespace BackendService.Services
{
    public class RoadmapService : IRoadmapService
    {
        private readonly IRoadmapRepository _roadmapRepository;
        private readonly IPathwayRepository _pathwayRepository;

        public RoadmapService(IRoadmapRepository roadmapRepository, IPathwayRepository pathwayRepository)
        {
            _roadmapRepository = roadmapRepository;
            _pathwayRepository = pathwayRepository;
        }

        public async Task<(string PathwayId, string GraphId)> SaveUserRoadmapAsync(string userId, UserRoadmapRequestDto request, CancellationToken cancellationToken = default)
        {
            // 1. Create RoadmapGraph
            var graph = new RoadmapGraph
            {
                Title = request.Title,
                GraphType = "community",
                OwnerUserId = userId,
                CreatedAt = DateTime.UtcNow
            };
            await _roadmapRepository.CreateGraphAsync(graph, cancellationToken);
            var graphId = graph.Id!;

            // 2. Create Nodes & Edges
            var nodeIds = new List<string>();
            var edgeIds = new List<string>();
            var tempToRealId = new Dictionary<string, string>();

            var nodesToInsert = new List<RoadmapNode>();
            if (request.Nodes != null)
            {
                foreach (var n in request.Nodes)
                {
                    var node = new RoadmapNode
                    {
                        GraphId = graphId,
                        Title = n.Title,
                        NodeType = n.NodeType,
                        ReferenceId = n.ReferenceId ?? string.Empty,
                        PositionX = n.PositionX,
                        PositionY = n.PositionY
                    };
                    nodesToInsert.Add(node);
                }

                await _roadmapRepository.CreateNodesAsync(nodesToInsert, cancellationToken);

                for (int i = 0; i < request.Nodes.Count; i++)
                {
                    var n = request.Nodes[i];
                    var realNode = nodesToInsert[i];
                    nodeIds.Add(realNode.Id!);
                    tempToRealId[n.Id] = realNode.Id!;
                }
            }

            var edgesToInsert = new List<RoadmapEdge>();
            if (request.Edges != null)
            {
                foreach (var e in request.Edges)
                {
                    if (tempToRealId.TryGetValue(e.SourceNodeId, out var src) && 
                        tempToRealId.TryGetValue(e.TargetNodeId, out var dst))
                    {
                        var edge = new RoadmapEdge
                        {
                            GraphId = graphId,
                            SourceNodeId = src,
                            TargetNodeId = dst
                        };
                        edgesToInsert.Add(edge);
                    }
                }

                await _roadmapRepository.CreateEdgesAsync(edgesToInsert, cancellationToken);
                edgeIds = edgesToInsert.Select(edge => edge.Id!).ToList();
            }

            // Update graph with node/edge IDs
            graph.NodeIds = nodeIds;
            graph.EdgeIds = edgeIds;
            await _roadmapRepository.UpdateGraphAsync(graphId, graph, cancellationToken);

            // 3. Create Pathway so it shows up in "My Roadmaps"
            var pathway = new Pathway
            {
                Title = request.Title,
                Slug = request.Title.ToLower().Replace(" ", "-") + "-" + Guid.NewGuid().ToString().Substring(0, 8),
                Description = "Roadmap được tạo bởi người dùng.",
                IsOfficial = false,
                CreatedBy = userId,
                RoadmapGraphId = graphId,
                CreatedAt = DateTime.UtcNow
            };
            await _pathwayRepository.CreatePathwayAsync(pathway, cancellationToken);

            return (pathway.Id!, graphId);
        }

        public async Task UpdateUserRoadmapAsync(string userId, string pathwayId, UserRoadmapRequestDto request, CancellationToken cancellationToken = default)
        {
            var pathway = await _pathwayRepository.GetPathwayByIdAndUserAsync(pathwayId, userId, cancellationToken);
            if (pathway == null) throw new KeyNotFoundException("Lộ trình không tồn tại hoặc bạn không có quyền chỉnh sửa.");

            var graphId = pathway.RoadmapGraphId;
            if (string.IsNullOrEmpty(graphId)) throw new InvalidOperationException("Không tìm thấy dữ liệu đồ thị.");

            // Update Pathway title
            await _pathwayRepository.UpdatePathwayTitleAsync(pathwayId, request.Title, cancellationToken);

            // Create placeholder Graph update object
            var graph = new RoadmapGraph
            {
                Id = graphId,
                Title = request.Title,
                GraphType = "community",
                OwnerUserId = userId,
                CreatedAt = DateTime.UtcNow // Placeholder or retrieved if needed, but this fully replaces/updates it.
            };

            // Delete old nodes/edges
            await _roadmapRepository.DeleteNodesAndEdgesByGraphIdAsync(graphId, cancellationToken);

            // Create new nodes/edges
            var nodeIds = new List<string>();
            var edgeIds = new List<string>();
            var tempToRealId = new Dictionary<string, string>();

            var nodesToInsert = new List<RoadmapNode>();
            if (request.Nodes != null)
            {
                foreach (var n in request.Nodes)
                {
                    var node = new RoadmapNode
                    {
                        GraphId = graphId,
                        Title = n.Title,
                        NodeType = n.NodeType,
                        ReferenceId = n.ReferenceId ?? string.Empty,
                        PositionX = n.PositionX,
                        PositionY = n.PositionY
                    };
                    nodesToInsert.Add(node);
                }

                await _roadmapRepository.CreateNodesAsync(nodesToInsert, cancellationToken);

                for (int i = 0; i < request.Nodes.Count; i++)
                {
                    var n = request.Nodes[i];
                    var realNode = nodesToInsert[i];
                    nodeIds.Add(realNode.Id!);
                    tempToRealId[n.Id] = realNode.Id!;
                }
            }

            var edgesToInsert = new List<RoadmapEdge>();
            if (request.Edges != null)
            {
                foreach (var e in request.Edges)
                {
                    if (tempToRealId.TryGetValue(e.SourceNodeId, out var src) && 
                        tempToRealId.TryGetValue(e.TargetNodeId, out var dst))
                    {
                        var edge = new RoadmapEdge
                        {
                            GraphId = graphId,
                            SourceNodeId = src,
                            TargetNodeId = dst
                        };
                        edgesToInsert.Add(edge);
                    }
                }

                await _roadmapRepository.CreateEdgesAsync(edgesToInsert, cancellationToken);
                edgeIds = edgesToInsert.Select(edge => edge.Id!).ToList();
            }

            // Update graph with new node/edge IDs
            graph.NodeIds = nodeIds;
            graph.EdgeIds = edgeIds;
            await _roadmapRepository.UpdateGraphAsync(graphId, graph, cancellationToken);
        }
    }
}
