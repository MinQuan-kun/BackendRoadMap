using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using BackendService.Data;
using BackendService.Models.Entities;
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

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var result = await _context.Users.DeleteOneAsync(u => u.Id == id);
            if (result.DeletedCount == 0) return NotFound("Người dùng không tồn tại.");
            return Ok(new { message = "Xóa tài khoản thành công." });
        }

        [HttpPost("users/{id}/approve-recruiter")]
        public async Task<IActionResult> ApproveRecruiter(string id)
        {
            var update = Builders<User>.Update.Set(u => u.Role, UserRole.Recruiter);
            await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return Ok(new { message = "Đã duyệt quyền Nhà tuyển dụng." });
        }

        [HttpPost("users/{id}/reject-recruiter")]
        public async Task<IActionResult> RejectRecruiter(string id)
        {
            var update = Builders<User>.Update.Set(u => u.Role, UserRole.User);
            await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            return Ok(new { message = "Đã từ chối quyền Nhà tuyển dụng." });
        }

        [HttpGet("stats")]
        public async Task<IActionResult> GetStats()
        {
            var totalUsers = await _context.Users.CountDocumentsAsync(_ => true);
            var totalPathways = await _context.Pathways.CountDocumentsAsync(_ => true);
            var totalCourses = await _context.Courses.CountDocumentsAsync(_ => true);
            var totalLessons = await _context.Lessons.CountDocumentsAsync(_ => true);

            return Ok(new
            {
                totalUsers,
                totalPathways,
                totalCourses,
                totalLessons
            });
        }

        [HttpGet("pathways")]
        public async Task<ActionResult<IEnumerable<Pathway>>> GetAllPathways() { return Ok(await _context.Pathways.Find(_ => true).ToListAsync()); }

        [HttpGet("pathways/{id}")]
        public async Task<ActionResult<Pathway>> GetPathwayById(string id) { return Ok(await _context.Pathways.Find(p => p.Id == id).FirstOrDefaultAsync()); }

        [HttpPost("pathways")]
        public async Task<ActionResult<Pathway>> CreatePathway([FromBody] Pathway pathway)
        {
            pathway.Id = null;
            await _context.Pathways.InsertOneAsync(pathway);
            return Ok(pathway);
        }

        [HttpPut("pathways/{id}")]
        public async Task<IActionResult> UpdatePathway(string id, [FromBody] Pathway pathway)
        {
            await _context.Pathways.ReplaceOneAsync(p => p.Id == id, pathway);
            return Ok();
        }

        [HttpDelete("pathways/{id}")]
        public async Task<IActionResult> DeletePathway(string id)
        {
            await _context.Pathways.DeleteOneAsync(p => p.Id == id);
            return Ok();
        }

        // --- Courses ---
        [HttpGet("courses")]
        public async Task<ActionResult<IEnumerable<Course>>> GetAllCourses() { return Ok(await _context.Courses.Find(_ => true).ToListAsync()); }

        [HttpGet("courses/{id}")]
        public async Task<ActionResult<Course>> GetCourseById(string id) { return Ok(await _context.Courses.Find(c => c.Id == id).FirstOrDefaultAsync()); }

        [HttpPost("courses")]
        public async Task<ActionResult<Course>> CreateCourse([FromBody] Course course)
        {
            course.Id = null;
            await _context.Courses.InsertOneAsync(course);
            return Ok(course);
        }

        [HttpPut("courses/{id}")]
        public async Task<IActionResult> UpdateCourse(string id, [FromBody] Course course)
        {
            await _context.Courses.ReplaceOneAsync(c => c.Id == id, course);
            return Ok();
        }

        [HttpDelete("courses/{id}")]
        public async Task<IActionResult> DeleteCourse(string id)
        {
            await _context.Courses.DeleteOneAsync(c => c.Id == id);
            return Ok();
        }

        // --- Modules ---
        [HttpGet("modules/{id}")]
        public async Task<ActionResult<Module>> GetModuleById(string id) { return Ok(await _context.Modules.Find(m => m.Id == id).FirstOrDefaultAsync()); }

        [HttpPost("modules")]
        public async Task<ActionResult<Module>> CreateModule([FromBody] Module module)
        {
            module.Id = null;
            await _context.Modules.InsertOneAsync(module);
            return Ok(module);
        }

        [HttpPut("modules/{id}")]
        public async Task<IActionResult> UpdateModule(string id, [FromBody] Module module)
        {
            await _context.Modules.ReplaceOneAsync(m => m.Id == id, module);
            return Ok();
        }

        [HttpDelete("modules/{id}")]
        public async Task<IActionResult> DeleteModule(string id)
        {
            await _context.Modules.DeleteOneAsync(m => m.Id == id);
            return Ok();
        }

        // --- Lessons ---
        [HttpGet("lessons/{id}")]
        public async Task<ActionResult<Lesson>> GetLessonById(string id) { return Ok(await _context.Lessons.Find(l => l.Id == id).FirstOrDefaultAsync()); }

        [HttpPost("lessons")]
        public async Task<ActionResult<Lesson>> CreateLesson([FromBody] Lesson lesson)
        {
            lesson.Id = null;
            await _context.Lessons.InsertOneAsync(lesson);
            return Ok(lesson);
        }

        [HttpPut("lessons/{id}")]
        public async Task<IActionResult> UpdateLesson(string id, [FromBody] Lesson lesson)
        {
            await _context.Lessons.ReplaceOneAsync(l => l.Id == id, lesson);
            return Ok();
        }

        [HttpDelete("lessons/{id}")]
        public async Task<IActionResult> DeleteLesson(string id)
        {
            await _context.Lessons.DeleteOneAsync(l => l.Id == id);
            return Ok();
        }
    }
}
