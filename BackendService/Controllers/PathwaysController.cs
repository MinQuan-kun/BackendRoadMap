using Microsoft.AspNetCore.Mvc;
using MongoDB.Driver;
using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Models.DTOs.Learning;
using Microsoft.AspNetCore.Authorization;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class PathwaysController : ControllerBase
    {
        private readonly MongoDbContext _context;

        public PathwaysController(MongoDbContext context)
        {
            _context = context;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PathwayDto>>> GetAllPathways()
        {
            var pathways = await _context.Pathways.Find(_ => true).ToListAsync();
            return Ok(pathways.Select(p => new PathwayDto
            {
                Id = p.Id,
                Title = p.Title,
                Slug = p.Slug,
                Description = p.Description,
                Thumbnail = p.Thumbnail,
                Difficulty = p.Difficulty,
                EstimatedHours = p.EstimatedHours,
                Tags = p.Tags,
                CourseIds = p.CourseIds,
                RoadmapGraphId = p.RoadmapGraphId,
                IsOfficial = p.IsOfficial
            }));
        }

        [HttpGet("{slug}")]
        public async Task<ActionResult<PathwayDto>> GetPathwayBySlug(string slug)
        {
            var pathway = await _context.Pathways.Find(p => p.Slug == slug).FirstOrDefaultAsync();
            if (pathway == null && slug.Length == 24 && System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[0-9a-fA-F]{24}$"))
            {
                pathway = await _context.Pathways.Find(p => p.Id == slug).FirstOrDefaultAsync();
            }
            if (pathway == null) return NotFound();

            return Ok(new PathwayDto
            {
                Id = pathway.Id,
                Title = pathway.Title,
                Slug = pathway.Slug,
                Description = pathway.Description,
                Thumbnail = pathway.Thumbnail,
                Difficulty = pathway.Difficulty,
                EstimatedHours = pathway.EstimatedHours,
                Tags = pathway.Tags,
                CourseIds = pathway.CourseIds,
                RoadmapGraphId = pathway.RoadmapGraphId,
                IsOfficial = pathway.IsOfficial
            });
        }
        [HttpGet("{slug}/content")]
        public async Task<ActionResult> GetPathwayContent(string slug)
        {
            var pathway = await _context.Pathways.Find(p => p.Slug == slug).FirstOrDefaultAsync();
            if (pathway == null && slug.Length == 24 && System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[0-9a-fA-F]{24}$"))
            {
                pathway = await _context.Pathways.Find(p => p.Id == slug).FirstOrDefaultAsync();
            }
            if (pathway == null) return NotFound();

            var courses = await _context.Courses.Find(c => pathway.CourseIds.Contains(c.Id!)).SortBy(c => c.Order).ToListAsync();
            
            var allModuleIds = courses.SelectMany(c => c.ModuleIds).ToList();
            var modules = await _context.Modules.Find(m => allModuleIds.Contains(m.Id!)).SortBy(m => m.Order).ToListAsync();

            var allLessonIds = modules.SelectMany(m => m.LessonIds).ToList();
            var lessons = await _context.Lessons.Find(l => allLessonIds.Contains(l.Id!)).ToListAsync();

            return Ok(new
            {
                pathway,
                courses = courses.Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    Modules = modules.Where(m => c.ModuleIds.Contains(m.Id!)).Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.Description,
                        Lessons = lessons.Where(l => m.LessonIds.Contains(l.Id!)).ToList()
                    }).ToList()
                }).ToList()
            });
        }
    }
}
