using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Models.DTOs;
using BackendService.Models.DTOs.User.Responses;
using BackendService.Mapping;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public AdminController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet("users")]
        public async Task<ActionResult<IEnumerable<UserResponseDto>>> GetAllUsers()
        {
            var users = await _context.Users.Find(_ => true).ToListAsync();
            var response = users.Select(UserToUserResponseDto.Transform).ToList();
            return Ok(response);
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequest request)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound("Người dùng không tồn tại.");

            if (request.UserName != null) user.UserName = request.UserName;
            if (request.FullName != null) user.FullName = request.FullName;
            if (request.Email != null) user.Email = request.Email;
            if (request.Role.HasValue) user.Role = request.Role.Value;

            await _context.Users.ReplaceOneAsync(u => u.Id == id, user);
            return Ok(UserToUserResponseDto.Transform(user));
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0) return NotFound("Người dùng không tồn tại.");
            return Ok(new { message = "Xóa tài khoản thành công." });
        }


        [HttpGet("nodes")]
        public async Task<ActionResult<IEnumerable<Node>>> GetAllNodes()
        {
            var nodes = await _context.Nodes.Find(_ => true).ToListAsync();
            return Ok(nodes);
        }

        [HttpGet("nodes/{id}")]
        public async Task<ActionResult<Node>> GetNodeById(string id)
        {
            var node = await _context.Nodes.Find(n => n.Id == id).FirstOrDefaultAsync();
            if (node == null) return NotFound("Node không tồn tại.");
            return Ok(node);
        }

        [HttpPost("nodes")]
        public async Task<ActionResult<Node>> CreateNode([FromBody] Node node)
        {
            node.Id = null;
            await _context.Nodes.InsertOneAsync(node);
            return CreatedAtAction(nameof(GetNodeById), new { id = node.Id }, node);
        }

        [HttpPut("nodes/{id}")]
        public async Task<IActionResult> UpdateNode(string id, [FromBody] Node updatedNode)
        {
            var existing = await _context.Nodes.Find(n => n.Id == id).FirstOrDefaultAsync();
            if (existing == null) return NotFound("Node không tồn tại.");

            updatedNode.Id = id;
            await _context.Nodes.ReplaceOneAsync(n => n.Id == id, updatedNode);
            return Ok(updatedNode);
        }

        [HttpDelete("nodes/{id}")]
        public async Task<IActionResult> DeleteNode(string id)
        {
            var result = await _context.Nodes.DeleteOneAsync(n => n.Id == id);
            if (result.DeletedCount == 0) return NotFound("Node không tồn tại.");
            return Ok(new { message = "Xóa node thành công." });
        }

        [HttpDelete("roadmaps/{id}")]
        public async Task<IActionResult> DeleteRoadmap(string id)
        {
            var result = await _context.Roadmaps.DeleteOneAsync(r => r.Id == id);
            if (result.DeletedCount == 0) return NotFound("Roadmap không tồn tại.");
            return Ok(new { message = "Xóa roadmap thành công." });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountDocumentsAsync(_ => true);
            var totalRoadmaps = await _context.Roadmaps.CountDocumentsAsync(_ => true);
            var totalNodes = await _context.Nodes.CountDocumentsAsync(_ => true);

            return Ok(new
            {
                totalUsers,
                totalRoadmaps,
                totalNodes
            });
        }
    }
}
