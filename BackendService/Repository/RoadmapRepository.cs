using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Repository.Interface;
using MongoDB.Driver;

namespace BackendService.Repository
{
    public class RoadmapRepository : IRoadmapRepository
    {
        private readonly MongoDbContext _context;

        public RoadmapRepository(MongoDbContext context)
        {
            _context = context;
        }

        public async Task CreateGraphAsync(RoadmapGraph graph, CancellationToken cancellationToken = default)
        {
            await _context.RoadmapGraphs.InsertOneAsync(graph, cancellationToken: cancellationToken);
        }

        public async Task UpdateGraphAsync(string id, RoadmapGraph graph, CancellationToken cancellationToken = default)
        {
            await _context.RoadmapGraphs.ReplaceOneAsync(g => g.Id == id, graph, cancellationToken: cancellationToken);
        }

        public async Task CreateNodesAsync(List<RoadmapNode> nodes, CancellationToken cancellationToken = default)
        {
            if (nodes.Any())
            {
                await _context.RoadmapNodes.InsertManyAsync(nodes, cancellationToken: cancellationToken);
            }
        }

        public async Task CreateEdgesAsync(List<RoadmapEdge> edges, CancellationToken cancellationToken = default)
        {
            if (edges.Any())
            {
                await _context.RoadmapEdges.InsertManyAsync(edges, cancellationToken: cancellationToken);
            }
        }

        public async Task DeleteNodesAndEdgesByGraphIdAsync(string graphId, CancellationToken cancellationToken = default)
        {
            await _context.RoadmapNodes.DeleteManyAsync(n => n.GraphId == graphId, cancellationToken: cancellationToken);
            await _context.RoadmapEdges.DeleteManyAsync(e => e.GraphId == graphId, cancellationToken: cancellationToken);
        }
    }
}
