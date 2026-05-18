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

        [HttpGet("courses")]
        public async Task<ActionResult<IEnumerable<object>>> GetAllCourses()
        {
            var courses = await _context.Courses.Find(_ => true).SortBy(c => c.Order).ToListAsync();
            return Ok(courses);
        }

        [HttpGet("followed")]
        [Authorize]
        public async Task<ActionResult<IEnumerable<object>>> GetFollowedPathways(CancellationToken cancellationToken)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId)) return Unauthorized();

            var user = await _context.Users.Find(u => u.Id == userId).FirstOrDefaultAsync(cancellationToken);
            if (user == null) return NotFound("User not found.");

            if (user.FollowedPathwayIds == null || user.FollowedPathwayIds.Count == 0)
            {
                return Ok(new List<object>());
            }

            var filter = Builders<Pathway>.Filter.In(p => p.Id, user.FollowedPathwayIds);
            var pathways = await _context.Pathways.Find(filter).ToListAsync(cancellationToken);

            var result = new List<object>();

            foreach (var pathway in pathways)
            {
                var courses = await _context.Courses.Find(c => pathway.CourseIds.Contains(c.Id!)).ToListAsync(cancellationToken);
                var allModuleIds = courses.SelectMany(c => c.ModuleIds).ToList();
                var modules = await _context.Modules.Find(m => allModuleIds.Contains(m.Id!)).ToListAsync(cancellationToken);
                
                var allLessonIds = modules.SelectMany(m => m.LessonIds).Distinct().ToList();
                var lessons = await _context.Lessons.Find(l => allLessonIds.Contains(l.Id!)).ToListAsync(cancellationToken);

                int totalLessons = allLessonIds.Count;
                int completedLessons = allLessonIds.Count(id => user.CompletedNodes.Contains(id));
                int skippedLessons = allLessonIds.Count(id => user.SkippedNodes.Contains(id));

                result.Add(new
                {
                    pathway.Id,
                    pathway.Title,
                    pathway.Slug,
                    pathway.Description,
                    pathway.Thumbnail,
                    pathway.Difficulty,
                    pathway.EstimatedHours,
                    pathway.IsOfficial,
                    TotalLessons = totalLessons,
                    CompletedLessons = completedLessons,
                    SkippedLessons = skippedLessons,
                    LessonsProgress = lessons.Select(lesson => {
                        var status = "not_started";
                        if (user.CompletedNodes.Contains(lesson.Id!)) status = "completed";
                        else if (user.SkippedNodes.Contains(lesson.Id!)) status = "skipped";
                        return new { 
                            LessonId = lesson.Id, 
                            Title = lesson.Title, 
                            Description = lesson.Description,
                            Status = status 
                        };
                    }).ToList()
                });
            }

            return Ok(result);
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<PathwayDto>>> GetPathways(
            [FromQuery] string? search, 
            [FromQuery] string? creatorId, 
            [FromQuery] string? engine, 
            [FromQuery] bool? includeOfficial,
            [FromQuery] string? type)
        {
            var filterBuilder = Builders<Pathway>.Filter;
            var filter = filterBuilder.Empty;

            if (!string.IsNullOrEmpty(search))
            {
                filter &= filterBuilder.Regex(p => p.Title, new MongoDB.Bson.BsonRegularExpression(search, "i"));
            }

            if (!string.IsNullOrEmpty(engine))
            {
                filter &= filterBuilder.Regex(p => p.Title, new MongoDB.Bson.BsonRegularExpression(engine, "i"));
            }

            if (type == "official")
            {
                filter &= filterBuilder.Eq(p => p.IsOfficial, true);
            }
            else if (type == "community")
            {
                filter &= filterBuilder.Eq(p => p.IsOfficial, false);
                filter &= filterBuilder.Eq(p => p.IsApproved, true);
            }
            else if (type == "recruiter")
            {
                var recruiters = await _context.Users.Find(u => u.Role == UserRole.Recruiter).ToListAsync();
                var recruiterIds = recruiters.Select(u => u.Id).Where(id => id != null).ToList();

                var jobs = await _context.Jobs.Find(j => !string.IsNullOrEmpty(j.RoadmapGraphId)).ToListAsync();
                var jobRoadmapIds = jobs.Select(j => j.RoadmapGraphId).Where(id => id != null).ToList();

                var recruiterFilter = filterBuilder.In(p => p.CreatedBy, recruiterIds) | 
                                      filterBuilder.In(p => p.RoadmapGraphId, jobRoadmapIds);
                
                filter &= recruiterFilter;
                filter &= filterBuilder.Eq(p => p.IsOfficial, false); // recruiter pathways are unofficial
            }
            else
            {
                if (!string.IsNullOrEmpty(creatorId))
                {
                    var creatorFilter = filterBuilder.Eq(p => p.CreatedBy, creatorId);
                    if (includeOfficial == true)
                    {
                        filter &= (creatorFilter | filterBuilder.Eq(p => p.IsOfficial, true));
                    }
                    else
                    {
                        filter &= creatorFilter;
                    }
                }
                else
                {
                    if (includeOfficial == true)
                    {
                        filter &= filterBuilder.Eq(p => p.IsOfficial, true);
                    }
                    else if (includeOfficial == false)
                    {
                        filter &= filterBuilder.Eq(p => p.IsOfficial, false);
                        filter &= filterBuilder.Eq(p => p.IsApproved, true);
                    }
                    else
                    {
                        filter &= (filterBuilder.Eq(p => p.IsOfficial, true) | filterBuilder.Eq(p => p.IsApproved, true));
                    }
                }
            }

            var pathways = await _context.Pathways.Find(filter).ToListAsync();
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
                IsOfficial = p.IsOfficial,
                IsApproved = p.IsApproved,
                CreatedBy = p.CreatedBy
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
                IsOfficial = pathway.IsOfficial,
                IsApproved = pathway.IsApproved,
                CreatedBy = pathway.CreatedBy
            });
        }
        [HttpGet("{slug}/content")]
        public async Task<ActionResult> GetPathwayContent(string slug)
        {
            Console.WriteLine($"[GetPathwayContent] START slug='{slug}' len={slug?.Length}");
            
            if (slug?.Length == 24 && System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[0-9a-fA-F]{24}$"))
            {
                var exists = await _context.Pathways.Find(p => p.Id == slug || p.Slug == slug || p.RoadmapGraphId == slug).AnyAsync()
                             || await _context.RoadmapGraphs.Find(g => g.Id == slug).AnyAsync();
                
                if (!exists)
                {
                    Console.WriteLine($"[GetPathwayContent] ID '{slug}' not found in database. Redirecting to appropriate official pathway...");
                    var job = await _context.Jobs.Find(j => j.RoadmapGraphId == slug || j.Id == slug).FirstOrDefaultAsync();
                    if (job != null && job.Title.Contains("Unreal", StringComparison.OrdinalIgnoreCase))
                    {
                        slug = "unreal-engine-beginner";
                        Console.WriteLine($"[GetPathwayContent] Job title contains Unreal. Redirected to '{slug}'");
                    }
                    else
                    {
                        slug = "unity-game-beginner";
                        Console.WriteLine($"[GetPathwayContent] Default redirection to '{slug}'");
                    }
                }
            }

            var pathway = await _context.Pathways.Find(p => p.Slug == slug).FirstOrDefaultAsync();
            if (pathway != null)
            {
                Console.WriteLine($"[GetPathwayContent] Found pathway by Slug: '{pathway.Id}'");
            }

            if (pathway == null && slug?.Length == 24 && System.Text.RegularExpressions.Regex.IsMatch(slug, @"^[0-9a-fA-F]{24}$"))
            {
                Console.WriteLine($"[GetPathwayContent] Slug '{slug}' matches 24-char hex format. Checking fallbacks...");
                pathway = await _context.Pathways.Find(p => p.Id == slug).FirstOrDefaultAsync();
                if (pathway != null) Console.WriteLine($"[GetPathwayContent] Fallback 1: Found Pathway by Id: '{pathway.Id}'");
                
                if (pathway == null)
                {
                    pathway = await _context.Pathways.Find(p => p.RoadmapGraphId == slug).FirstOrDefaultAsync();
                    if (pathway != null) Console.WriteLine($"[GetPathwayContent] Fallback 2: Found Pathway by RoadmapGraphId (string): '{pathway.Id}'");
                }

                // 3. Try finding Pathway by RoadmapGraphId as ObjectId (in case stored as ObjectId in DB)
                if (pathway == null && MongoDB.Bson.ObjectId.TryParse(slug, out var graphObjId))
                {
                    var filter = Builders<Pathway>.Filter.Eq("roadmap_graph_id", graphObjId);
                    pathway = await _context.Pathways.Find(filter).FirstOrDefaultAsync();
                    if (pathway != null) Console.WriteLine($"[GetPathwayContent] Fallback 3: Found Pathway by RoadmapGraphId (ObjectId): '{pathway.Id}'");
                }

                // 4. Try finding if a RoadmapGraph exists with this ID, and dynamically mock a Pathway
                if (pathway == null)
                {
                    Console.WriteLine($"[GetPathwayContent] Pathway still null. Looking up RoadmapGraph with ID '{slug}'...");
                    var graph = await _context.RoadmapGraphs.Find(g => g.Id == slug).FirstOrDefaultAsync();
                    if (graph == null && MongoDB.Bson.ObjectId.TryParse(slug, out var gObjId))
                    {
                        var graphFilter = Builders<RoadmapGraph>.Filter.Eq("Id", gObjId);
                        graph = await _context.RoadmapGraphs.Find(graphFilter).FirstOrDefaultAsync();
                    }

                    if (graph != null)
                    {
                        Console.WriteLine($"[GetPathwayContent] Fallback 4: Found RoadmapGraph '{graph.Id}'. Creating synthetic Pathway!");
                        pathway = new Pathway
                        {
                            Id = slug,
                            Title = graph.Title,
                            Slug = graph.Title.ToLower().Replace(" ", "-") + "-" + slug.Substring(0, 6),
                            Description = "Lộ trình tuyển dụng dành riêng cho bạn.",
                            RoadmapGraphId = graph.Id,
                            CourseIds = new List<string>()
                        };
                    }
                    else
                    {
                        Console.WriteLine($"[GetPathwayContent] RoadmapGraph not found for ID '{slug}'");
                    }
                }
            }
            else if (pathway == null)
            {
                Console.WriteLine($"[GetPathwayContent] Slug '{slug}' is NOT 24-char hex format. Skipping fallbacks.");
            }

            if (pathway == null)
            {
                // Fallback: Check if it's a Course ID
                var course = await _context.Courses.Find(c => c.Id == slug).FirstOrDefaultAsync();
                if (course != null) return await GetCourseContent(slug);

                // Fallback: Check if it's a Task ID
                var task = await _context.Tasks.Find(t => t.Id == slug).FirstOrDefaultAsync();
                if (task != null)
                {
                    // If it's a task, we need to find the lesson, module, and course it belongs to
                    // For now, let's just return a minimal structure so the frontend doesn't crash
                    return Ok(new
                    {
                        pathway = new { id = task.Id, title = task.Title },
                        courses = new List<object>
                        {
                            new
                            {
                                id = "single-task-course",
                                title = "Nhiệm vụ lẻ",
                                modules = new List<object>
                                {
                                    new
                                    {
                                        id = "single-task-module",
                                        title = "Thông tin nhiệm vụ",
                                        lessons = new List<object>
                                        {
                                            new
                                            {
                                                id = "single-task-lesson",
                                                title = task.Title,
                                                tasks = new List<object> { task }
                                            }
                                        }
                                    }
                                }
                            }
                        }
                    });
                }
                return NotFound();
            }

            var courses = await _context.Courses.Find(c => pathway.CourseIds.Contains(c.Id!)).SortBy(c => c.Order).ToListAsync();
            
            var allModuleIds = courses.SelectMany(c => c.ModuleIds).ToList();
            var modules = await _context.Modules.Find(m => allModuleIds.Contains(m.Id!)).SortBy(m => m.Order).ToListAsync();

            var allLessonIds = modules.SelectMany(m => m.LessonIds).ToList();
            var lessons = await _context.Lessons.Find(l => allLessonIds.Contains(l.Id!)).ToListAsync();

            var allTaskIds = lessons.SelectMany(l => l.TaskIds).ToList();
            var tasks = await _context.Tasks.Find(t => allTaskIds.Contains(t.Id!)).ToListAsync();

            return Ok(new
            {
                pathway,
                courses = courses.Select(c => new
                {
                    c.Id,
                    c.Title,
                    c.Description,
                    c.Thumbnail,
                    c.CoverUrl,
                    c.Difficulty,
                    c.EstimatedHours,
                    c.XPReward,
                    Modules = modules.Where(m => c.ModuleIds.Contains(m.Id!)).Select(m => new
                    {
                        m.Id,
                        m.Title,
                        m.Description,
                        Lessons = lessons.Where(l => m.LessonIds.Contains(l.Id!)).Select(l => new
                        {
                            l.Id,
                            l.Title,
                            l.Description,
                            l.VideoUrl,
                            l.ContentBlocks,
                            l.Resources,
                            l.Prerequisites,
                            Tasks = tasks.Where(t => l.TaskIds.Contains(t.Id!)).ToList()
                        }).ToList()
                    }).ToList()
                }).ToList()
            });
        }
        [HttpGet("course/{id}")]
        public async Task<ActionResult> GetCourseContent(string id)
        {
            var course = await _context.Courses.Find(c => c.Id == id).FirstOrDefaultAsync();
            if (course == null) return NotFound();

            var modules = await _context.Modules.Find(m => course.ModuleIds.Contains(m.Id!)).SortBy(m => m.Order).ToListAsync();
            var allLessonIds = modules.SelectMany(m => m.LessonIds).ToList();
            var lessons = await _context.Lessons.Find(l => allLessonIds.Contains(l.Id!)).ToListAsync();
            var allTaskIds = lessons.SelectMany(l => l.TaskIds).ToList();
            var tasks = await _context.Tasks.Find(t => allTaskIds.Contains(t.Id!)).ToListAsync();

            return Ok(new
            {
                pathway = new { title = course.Title },
                courses = new List<object> {
                    new {
                        course.Id,
                        course.Title,
                        course.Description,
                        course.Thumbnail,
                        course.CoverUrl,
                        course.Difficulty,
                        course.EstimatedHours,
                        course.XPReward,
                        Modules = modules.Select(m => new {
                            m.Id,
                            m.Title,
                            m.Description,
                            Lessons = lessons.Where(l => m.LessonIds.Contains(l.Id!)).Select(l => new {
                                l.Id,
                                l.Title,
                                l.Description,
                                l.VideoUrl,
                                l.ContentBlocks,
                                l.Resources,
                                l.Prerequisites,
                                Tasks = tasks.Where(t => l.TaskIds.Contains(t.Id!)).ToList()
                            }).ToList()
                        }).ToList()
                    }
                }
            });
        }
    }
}
