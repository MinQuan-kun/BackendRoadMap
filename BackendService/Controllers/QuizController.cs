using BackendService.Data;
using BackendService.Models.DTOs.Question;
using BackendService.Models.Entities;
using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/quiz")]
    public class QuizController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public QuizController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("questions")]
        public async Task<ActionResult<List<QuestionBank>>> GetQuestions()
        {
            var questions = await _context.Questions.Find(_ => true).ToListAsync();
            return Ok(questions);
        }

        [HttpPost("submit")]
        public async Task<IActionResult> SubmitQuiz([FromBody] QuizSubmissionDto submission)
        {
            var filter = Builders<User>.Filter.Eq(u => u.Id, submission.UserId);

            var update = Builders<User>.Update.Set(u => u.InterestedNodes, submission.SelectedNodeIds);

            if (submission.SkipBasics)
            {
                var basicNodeIds = await _context.Nodes
                    .Find(n => n.Category == "Language" || n.Category == "Language Syntax")
                    .Project(n => n.Id)
                    .ToListAsync();

                update = update.AddToSetEach(u => u.CompletedNodes, basicNodeIds);
            }

            var result = await _context.Users.UpdateOneAsync(filter, update);

            if (result.MatchedCount == 0) return NotFound("Không tìm thấy User!");

            var allNodes = await _context.Nodes.Find(_ => true).ToListAsync();
            var nodeMap = allNodes.Where(n => n.Id != null).ToDictionary(n => n.Id!, n => n);

            var expandedNodeIds = new HashSet<string>(submission.SelectedNodeIds);

            foreach (var nodeId in submission.SelectedNodeIds)
            {
                var currentId = nodeId;
                while (!string.IsNullOrWhiteSpace(currentId) && nodeMap.ContainsKey(currentId))
                {
                    expandedNodeIds.Add(currentId);
                    var parentId = nodeMap[currentId].ParentId;
                    if (string.IsNullOrWhiteSpace(parentId) || expandedNodeIds.Contains(parentId))
                        break;
                    currentId = parentId;
                }
            }

            // Đệ quy thêm tất cả các node con (descendants) của các node đã chọn
            var queue = new Queue<string>(submission.SelectedNodeIds);
            while (queue.Count > 0)
            {
                var currentId = queue.Dequeue();
                var children = allNodes.Where(n => n.ParentId == currentId).ToList();
                foreach (var child in children)
                {
                    if (child.Id != null && !expandedNodeIds.Contains(child.Id))
                    {
                        expandedNodeIds.Add(child.Id);
                        queue.Enqueue(child.Id);
                    }
                }
            }

            var roadmapNodes = allNodes
                .Where(n => n.Id != null && expandedNodeIds.Contains(n.Id))
                .ToList();
            var nodesLayout = CalculateTreeLayout(roadmapNodes);

            // ═══════════════════════ Xây dựng Roadmap ═══════════════════════
            var roadmap = new Roadmap
            {
                Title = "Lộ trình học tập cá nhân hóa",
                Engine = "Custom",
                Description = "Roadmap được tạo tự động từ kết quả bài khảo sát đầu vào.",
                Difficulty = submission.SkipBasics ? "Intermediate" : "Beginner",
                CreatorId = submission.UserId,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow,
                NodesLayout = nodesLayout
            };

            await _context.Roadmaps.InsertOneAsync(roadmap);

            return Ok(new
            {
                message = "Lộ trình cá nhân hóa của bạn đã được khởi tạo!",
                roadmapId = roadmap.Id,
                nodeCount = nodesLayout.Count
            });
        }

        private List<NodeLayout> CalculateTreeLayout(List<Node> nodes)
        {
            if (nodes.Count == 0) return new List<NodeLayout>();

            var nodeMap = nodes.Where(n => n.Id != null).ToDictionary(n => n.Id!, n => n);
            var nodeIds = new HashSet<string>(nodeMap.Keys);

            var rootNodes = nodes
                .Where(n => string.IsNullOrWhiteSpace(n.ParentId) || !nodeIds.Contains(n.ParentId))
                .ToList();

            var childrenMap = new Dictionary<string, List<Node>>();
            foreach (var node in nodes)
            {
                if (!string.IsNullOrWhiteSpace(node.ParentId) && nodeIds.Contains(node.ParentId))
                {
                    if (!childrenMap.ContainsKey(node.ParentId))
                        childrenMap[node.ParentId] = new List<Node>();
                    childrenMap[node.ParentId].Add(node);
                }
            }

            var result = new List<NodeLayout>();
            const double horizontalSpacing = 280;
            const double verticalSpacing = 180;
            double currentX = 0;

            foreach (var root in rootNodes)
            {
                var subtreeWidth = CalculateSubtreeWidth(root.Id!, childrenMap);
                var startX = currentX + (subtreeWidth * horizontalSpacing) / 2;

                LayoutSubtree(root, startX, 80, 0, childrenMap, result, horizontalSpacing, verticalSpacing);
                currentX += subtreeWidth * horizontalSpacing;
            }

            return result;
        }

        private int CalculateSubtreeWidth(string nodeId, Dictionary<string, List<Node>> childrenMap)
        {
            if (!childrenMap.ContainsKey(nodeId) || childrenMap[nodeId].Count == 0)
                return 1;

            return childrenMap[nodeId].Sum(child => CalculateSubtreeWidth(child.Id!, childrenMap));
        }

        private void LayoutSubtree(
            Node node,
            double centerX,
            double y,
            int depth,
            Dictionary<string, List<Node>> childrenMap,
            List<NodeLayout> result,
            double hSpacing,
            double vSpacing)
        {
            result.Add(new NodeLayout
            {
                NodeId = node.Id!,
                X = centerX,
                Y = y
            });

            if (!childrenMap.ContainsKey(node.Id!) || childrenMap[node.Id!].Count == 0)
                return;

            var children = childrenMap[node.Id!];
            var totalWidth = children.Sum(c => CalculateSubtreeWidth(c.Id!, childrenMap));
            var startX = centerX - (totalWidth * hSpacing) / 2;

            foreach (var child in children)
            {
                var childWidth = CalculateSubtreeWidth(child.Id!, childrenMap);
                var childCenterX = startX + (childWidth * hSpacing) / 2;

                LayoutSubtree(child, childCenterX, y + vSpacing, depth + 1, childrenMap, result, hSpacing, vSpacing);
                startX += childWidth * hSpacing;
            }
        }
    }
}