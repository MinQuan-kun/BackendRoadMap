using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using MongoDB.Driver;
using BackendService.Data;
using BackendService.Models.Entities;
using BackendService.Models.DTOs.User.Responses;
using BackendService.Models.DTOs.User.Requests;
using BackendService.Mapping;
using Microsoft.Extensions.Options;
using BackendService.Configurations;
using BackendService.Services.Interface;
using BackendService.Models.DTOs.Admin;

namespace BackendService.Controllers
{
    [ApiController]
    [Route("api/admin")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly MongoDbContext _context;
        private readonly ICloudinaryService _cloudinaryService;
        private readonly string _baseFolder;

        public AdminController(MongoDbContext context, ICloudinaryService cloudinaryService, IOptions<CloudinarySettings> config)
        {
            _context = context;
            _cloudinaryService = cloudinaryService;
            _baseFolder = config.Value.BaseFolder;
        }

        [HttpPost("upload")]
        public async Task<IActionResult> UploadImage(IFormFile file, [FromQuery] string subFolder = "general")
        {
            var result = await _cloudinaryService.UploadImageAsync(file, subFolder);
            if (result.Error != null) return BadRequest(result.Error.Message);

            return Ok(new { url = result.SecureUrl.ToString(), publicId = result.PublicId });
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
            var update = Builders<User>.Update
                .Set(u => u.Role, UserRole.Recruiter)
                .Set(u => u.IsRecruiterVerified, true)
                .Set(u => u.Status, UserStatus.Active)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            if (result.MatchedCount == 0) return NotFound("Người dùng không tồn tại.");

            return Ok(new { message = "Đã duyệt quyền Nhà tuyển dụng." });
        }

        [HttpPost("users/{id}/reject-recruiter")]
        public async Task<IActionResult> RejectRecruiter(string id)
        {
            var update = Builders<User>.Update
                .Set(u => u.Role, UserRole.User)
                .Set(u => u.IsRecruiterVerified, false)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            if (result.MatchedCount == 0) return NotFound("Người dùng không tồn tại.");

            return Ok(new { message = "Đã từ chối quyền Nhà tuyển dụng." });
        }

        [HttpPut("users/{id}")]
        public async Task<IActionResult> UpdateUser(string id, [FromBody] UpdateUserRequestDto request)
        {
            var user = await _context.Users.Find(u => u.Id == id).FirstOrDefaultAsync();
            if (user == null) return NotFound("Người dùng không tồn tại.");

            if (!string.IsNullOrEmpty(request.UserName)) user.UserName = request.UserName;
            if (!string.IsNullOrEmpty(request.Email)) user.Email = request.Email;
            if (!string.IsNullOrEmpty(request.DisplayName)) user.DisplayName = request.DisplayName;
            if (!string.IsNullOrEmpty(request.Bio)) user.Bio = request.Bio;
            if (request.Role.HasValue) user.Role = request.Role.Value;

            user.UpdatedAt = DateTime.UtcNow;

            await _context.Users.ReplaceOneAsync(u => u.Id == id, user);

            var response = UserToUserResponseDto.Transform(user);
            return Ok(response);
        }

        [HttpPost("users/{id}/reset-password")]
        public async Task<IActionResult> ResetPassword(string id, [FromBody] AdminResetPasswordRequestDto request)
        {
            if (string.IsNullOrEmpty(request.NewPassword))
            {
                return BadRequest("Mật khẩu mới không được để trống.");
            }

            if (request.NewPassword.Length < 8)
            {
                return BadRequest("Mật khẩu phải có ít nhất 8 ký tự.");
            }

            var passwordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
            
            var update = Builders<User>.Update
                .Set(u => u.PasswordHash, passwordHash)
                .Set(u => u.UpdatedAt, DateTime.UtcNow);

            var result = await _context.Users.UpdateOneAsync(u => u.Id == id, update);
            if (result.MatchedCount == 0) return NotFound("Người dùng không tồn tại.");

            return Ok(new { message = "Đã đặt lại mật khẩu thành công." });
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

        [HttpGet("pathways/full/{id}")]
        public async Task<ActionResult<object>> GetFullPathway(string id)
        {
            var pathway = await _context.Pathways.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (pathway == null) return NotFound();

            var courseData = new List<object>();
            if (pathway.CourseIds != null && pathway.CourseIds.Count > 0)
            {
                var allCourses = await _context.Courses.Find(Builders<Course>.Filter.In(c => c.Id, pathway.CourseIds)).ToListAsync();
                var orderedCourses = pathway.CourseIds
                    .Select(cId => allCourses.FirstOrDefault(c => c.Id == cId))
                    .Where(c => c != null)
                    .ToList();

                var allModuleIds = orderedCourses.SelectMany(c => c!.ModuleIds).Distinct().ToList();
                var allModules = allModuleIds.Count > 0 
                    ? await _context.Modules.Find(Builders<Module>.Filter.In(m => m.Id, allModuleIds)).ToListAsync()
                    : new List<Module>();

                var allLessonIds = allModules.SelectMany(m => m.LessonIds).Distinct().ToList();
                var allLessons = allLessonIds.Count > 0
                    ? await _context.Lessons.Find(Builders<Lesson>.Filter.In(l => l.Id, allLessonIds)).ToListAsync()
                    : new List<Lesson>();

                // Fetch all tasks for all lessons
                var allTaskIds = allLessons.SelectMany(l => l.TaskIds).Distinct().ToList();
                var allTasks = allTaskIds.Count > 0
                    ? await _context.Tasks.Find(Builders<LearningTask>.Filter.In(t => t.Id, allTaskIds)).ToListAsync()
                    : new List<LearningTask>();

                foreach (var course in orderedCourses)
                {
                    var modules = new List<object>();
                    foreach (var mId in course!.ModuleIds)
                    {
                        var module = allModules.FirstOrDefault(m => m.Id == mId);
                        if (module == null) continue;

                        var lessons = allLessons.Where(l => module.LessonIds.Contains(l.Id!)).Select(l => new
                        {
                            l.Id,
                            l.Title,
                            l.Description,
                            l.VideoUrl,
                            l.Difficulty,
                            l.XPReward,
                            Tasks = allTasks.Where(t => l.TaskIds.Contains(t.Id!)).ToList()
                        }).ToList();

                        modules.Add(new
                        {
                            Id = module.Id,
                            Title = module.Title,
                            Description = module.Description,
                            Lessons = lessons
                        });
                    }

                    courseData.Add(new
                    {
                        Id = course.Id,
                        Title = course.Title,
                        Description = course.Description,
                        Modules = modules
                    });
                }
            }

            object? graphData = null;
            if (!string.IsNullOrEmpty(pathway.RoadmapGraphId))
            {
                var graph = await _context.RoadmapGraphs.Find(g => g.Id == pathway.RoadmapGraphId).FirstOrDefaultAsync();
                if (graph != null)
                {
                    var nodes = await _context.RoadmapNodes.Find(Builders<RoadmapNode>.Filter.In(n => n.Id, graph.NodeIds)).ToListAsync();
                    var edges = await _context.RoadmapEdges.Find(Builders<RoadmapEdge>.Filter.In(e => e.Id, graph.EdgeIds)).ToListAsync();
                    graphData = new { nodes, edges };
                }
            }

            return Ok(new
            {
                Id = pathway.Id,
                Title = pathway.Title,
                Slug = pathway.Slug,
                Description = pathway.Description,
                Thumbnail = pathway.Thumbnail,
                Difficulty = pathway.Difficulty,
                EstimatedHours = pathway.EstimatedHours,
                Tags = pathway.Tags,
                IsOfficial = pathway.IsOfficial,
                Courses = courseData,
                Graph = graphData
            });
        }

        [HttpPost("pathways/full")]
        public async Task<ActionResult<Pathway>> CreateFullPathway([FromBody] FullPathwayRequestDto request)
        {
            var courseIds = new List<string>();
            var tempToRealCourseId = new Dictionary<string, string>();

            int courseOrder = 0;
            foreach (var cReq in request.Courses)
            {
                var moduleIds = new List<string>();
                int moduleOrder = 0;

                foreach (var mReq in cReq.Modules)
                {
                    var lessonIds = new List<string>();
                    foreach (var lReq in mReq.Lessons)
                    {
                        // Save tasks for this lesson
                        var taskIds = new List<string>();
                        if (lReq.Tasks != null)
                        {
                            foreach (var tReq in lReq.Tasks)
                            {
                                var task = new LearningTask
                                {
                                    Title = tReq.Title,
                                    Description = tReq.Description,
                                    TaskType = tReq.TaskType ?? "practice",
                                    Difficulty = tReq.Difficulty ?? "easy",
                                    XPReward = tReq.XPReward,
                                    MediaUrl = tReq.MediaUrl,
                                    MediaType = tReq.MediaType
                                };
                                await _context.Tasks.InsertOneAsync(task);
                                taskIds.Add(task.Id!);
                            }
                        }

                        var lesson = new Lesson
                        {
                            Title = lReq.Title,
                            Description = $"Nội dung bài học {lReq.Title} đang được cập nhật...",
                            Difficulty = lReq.Difficulty ?? "easy",
                            XPReward = lReq.XPReward > 0 ? lReq.XPReward : 10,
                            TaskIds = taskIds
                        };
                        await _context.Lessons.InsertOneAsync(lesson);
                        lessonIds.Add(lesson.Id!);
                    }

                    var module = new Module
                    {
                        Title = mReq.Title,
                        Description = mReq.Description,
                        Order = moduleOrder++,
                        LessonIds = lessonIds
                    };
                    await _context.Modules.InsertOneAsync(module);
                    moduleIds.Add(module.Id!);
                }

                var course = new Course
                {
                    Title = cReq.Title,
                    Description = cReq.Description,
                    Order = courseOrder++,
                    ModuleIds = moduleIds
                };
                await _context.Courses.InsertOneAsync(course);
                courseIds.Add(course.Id!);
                tempToRealCourseId[cReq.Id] = course.Id!;
            }

            string? graphId = null;
            if (request.Graph != null)
            {
                var nodeIds = new List<string>();
                var tempCourseIdToNodeId = new Dictionary<string, string>();

                foreach (var nReq in request.Graph.Nodes)
                {
                    if (tempToRealCourseId.TryGetValue(nReq.CourseId, out var realId))
                    {
                        var node = new RoadmapNode
                        {
                            Title = request.Courses.Find(c => c.Id == nReq.CourseId)?.Title ?? "Node",
                            ReferenceId = realId,
                            NodeType = "course",
                            PositionX = nReq.X,
                            PositionY = nReq.Y
                        };
                        await _context.RoadmapNodes.InsertOneAsync(node);
                        nodeIds.Add(node.Id!);
                        tempCourseIdToNodeId[nReq.CourseId] = node.Id!;
                    }
                }

                var edgeIds = new List<string>();
                foreach (var eReq in request.Graph.Edges)
                {
                    if (tempCourseIdToNodeId.TryGetValue(eReq.SourceId, out var sourceNodeId) && 
                        tempCourseIdToNodeId.TryGetValue(eReq.TargetId, out var targetNodeId))
                    {
                        var edge = new RoadmapEdge
                        {
                            SourceNodeId = sourceNodeId,
                            TargetNodeId = targetNodeId
                        };
                        await _context.RoadmapEdges.InsertOneAsync(edge);
                        edgeIds.Add(edge.Id!);
                    }
                }

                var graph = new RoadmapGraph
                {
                    Title = request.Title + " Graph",
                    NodeIds = nodeIds,
                    EdgeIds = edgeIds
                };
                await _context.RoadmapGraphs.InsertOneAsync(graph);
                graphId = graph.Id;

                await _context.RoadmapNodes.UpdateManyAsync(
                    Builders<RoadmapNode>.Filter.In(n => n.Id, nodeIds),
                    Builders<RoadmapNode>.Update.Set(n => n.GraphId, graphId!)
                );
                if (edgeIds.Count > 0)
                {
                    await _context.RoadmapEdges.UpdateManyAsync(
                        Builders<RoadmapEdge>.Filter.In(e => e.Id, edgeIds),
                        Builders<RoadmapEdge>.Update.Set(e => e.GraphId, graphId!)
                    );
                }
            }

            var pathway = new Pathway
            {
                Title = request.Title,
                Slug = request.Slug,
                Description = request.Description,
                Thumbnail = request.Thumbnail,
                Difficulty = request.Difficulty,
                EstimatedHours = request.EstimatedHours,
                Tags = request.Tags,
                IsOfficial = request.IsOfficial,
                CourseIds = courseIds,
                RoadmapGraphId = graphId
            };

            await _context.Pathways.InsertOneAsync(pathway);
            return Ok(pathway);
        }

        [HttpPut("pathways/full/{id}")]
        public async Task<ActionResult<Pathway>> UpdateFullPathway(string id, [FromBody] FullPathwayRequestDto request)
        {
            var existingPathway = await _context.Pathways.Find(p => p.Id == id).FirstOrDefaultAsync();
            if (existingPathway == null) return NotFound("Pathway not found");

            if (existingPathway.CourseIds != null && existingPathway.CourseIds.Count > 0)
            {
                var courses = await _context.Courses.Find(Builders<Course>.Filter.In(c => c.Id, existingPathway.CourseIds)).ToListAsync();
                var moduleIds = courses.SelectMany(c => c.ModuleIds).Distinct().ToList();

                if (moduleIds.Count > 0)
                {
                    var modules = await _context.Modules.Find(Builders<Module>.Filter.In(m => m.Id, moduleIds)).ToListAsync();
                    var lessonIds = modules.SelectMany(m => m.LessonIds).Distinct().ToList();

                    if (lessonIds.Count > 0)
                    {
                        var oldLessons = await _context.Lessons.Find(Builders<Lesson>.Filter.In(l => l.Id, lessonIds)).ToListAsync();
                        var oldTaskIds = oldLessons.SelectMany(l => l.TaskIds).Distinct().ToList();
                        if (oldTaskIds.Count > 0)
                        {
                            await _context.Tasks.DeleteManyAsync(Builders<LearningTask>.Filter.In(t => t.Id, oldTaskIds));
                        }
                        await _context.Lessons.DeleteManyAsync(Builders<Lesson>.Filter.In(l => l.Id, lessonIds));
                    }
                    await _context.Modules.DeleteManyAsync(Builders<Module>.Filter.In(m => m.Id, moduleIds));
                }
                await _context.Courses.DeleteManyAsync(Builders<Course>.Filter.In(c => c.Id, existingPathway.CourseIds));
            }

            if (!string.IsNullOrEmpty(existingPathway.RoadmapGraphId))
            {
                var graph = await _context.RoadmapGraphs.Find(g => g.Id == existingPathway.RoadmapGraphId).FirstOrDefaultAsync();
                if (graph != null)
                {
                    if (graph.NodeIds != null) await _context.RoadmapNodes.DeleteManyAsync(n => graph.NodeIds.Contains(n.Id!));
                    if (graph.EdgeIds != null) await _context.RoadmapEdges.DeleteManyAsync(e => graph.EdgeIds.Contains(e.Id!));
                    await _context.RoadmapGraphs.DeleteOneAsync(g => g.Id == existingPathway.RoadmapGraphId);
                }
            }

            var courseIds = new List<string>();
            var tempToRealCourseId = new Dictionary<string, string>();

            int courseOrder = 0;
            foreach (var cReq in request.Courses)
            {
                var moduleIds = new List<string>();
                int moduleOrder = 0;

                foreach (var mReq in cReq.Modules)
                {
                    var lessonIds = new List<string>();
                    foreach (var lReq in mReq.Lessons)
                    {
                        // Save tasks for this lesson
                        var taskIds = new List<string>();
                        if (lReq.Tasks != null)
                        {
                            foreach (var tReq in lReq.Tasks)
                            {
                                var task = new LearningTask
                                {
                                    Title = tReq.Title,
                                    Description = tReq.Description,
                                    TaskType = tReq.TaskType ?? "practice",
                                    Difficulty = tReq.Difficulty ?? "easy",
                                    XPReward = tReq.XPReward,
                                    MediaUrl = tReq.MediaUrl,
                                    MediaType = tReq.MediaType
                                };
                                await _context.Tasks.InsertOneAsync(task);
                                taskIds.Add(task.Id!);
                            }
                        }

                        var lesson = new Lesson
                        {
                            Title = lReq.Title,
                            Description = $"Nội dung bài học {lReq.Title} đang được cập nhật...",
                            Difficulty = lReq.Difficulty ?? "easy",
                            XPReward = lReq.XPReward > 0 ? lReq.XPReward : 10,
                            TaskIds = taskIds
                        };
                        await _context.Lessons.InsertOneAsync(lesson);
                        lessonIds.Add(lesson.Id!);
                    }

                    var module = new Module
                    {
                        Title = mReq.Title,
                        Description = mReq.Description,
                        Order = moduleOrder++,
                        LessonIds = lessonIds
                    };
                    await _context.Modules.InsertOneAsync(module);
                    moduleIds.Add(module.Id!);
                }

                var course = new Course
                {
                    Title = cReq.Title,
                    Description = cReq.Description,
                    Order = courseOrder++,
                    ModuleIds = moduleIds
                };
                await _context.Courses.InsertOneAsync(course);
                courseIds.Add(course.Id!);
                tempToRealCourseId[cReq.Id] = course.Id!;
            }

            string? graphId = null;
            if (request.Graph != null)
            {
                var nodeIds = new List<string>();
                var tempCourseIdToNodeId = new Dictionary<string, string>();

                foreach (var nReq in request.Graph.Nodes)
                {
                    if (tempToRealCourseId.TryGetValue(nReq.CourseId, out var realId))
                    {
                        var node = new RoadmapNode
                        {
                            Title = request.Courses.Find(c => c.Id == nReq.CourseId)?.Title ?? "Node",
                            ReferenceId = realId,
                            NodeType = "course",
                            PositionX = nReq.X,
                            PositionY = nReq.Y
                        };
                        await _context.RoadmapNodes.InsertOneAsync(node);
                        nodeIds.Add(node.Id!);
                        tempCourseIdToNodeId[nReq.CourseId] = node.Id!;
                    }
                }

                var edgeIds = new List<string>();
                foreach (var eReq in request.Graph.Edges)
                {
                    if (tempCourseIdToNodeId.TryGetValue(eReq.SourceId, out var sourceNodeId) && 
                        tempCourseIdToNodeId.TryGetValue(eReq.TargetId, out var targetNodeId))
                    {
                        var edge = new RoadmapEdge
                        {
                            SourceNodeId = sourceNodeId,
                            TargetNodeId = targetNodeId
                        };
                        await _context.RoadmapEdges.InsertOneAsync(edge);
                        edgeIds.Add(edge.Id!);
                    }
                }

                var graph = new RoadmapGraph
                {
                    Title = request.Title + " Graph",
                    NodeIds = nodeIds,
                    EdgeIds = edgeIds
                };
                await _context.RoadmapGraphs.InsertOneAsync(graph);
                graphId = graph.Id;

                await _context.RoadmapNodes.UpdateManyAsync(
                    Builders<RoadmapNode>.Filter.In(n => n.Id, nodeIds),
                    Builders<RoadmapNode>.Update.Set(n => n.GraphId, graphId!)
                );
                if (edgeIds.Count > 0)
                {
                    await _context.RoadmapEdges.UpdateManyAsync(
                        Builders<RoadmapEdge>.Filter.In(e => e.Id, edgeIds),
                        Builders<RoadmapEdge>.Update.Set(e => e.GraphId, graphId!)
                    );
                }
            }

            existingPathway.Title = request.Title;
            existingPathway.Slug = request.Slug;
            existingPathway.Description = request.Description;
            existingPathway.Thumbnail = request.Thumbnail;
            existingPathway.Difficulty = request.Difficulty;
            existingPathway.EstimatedHours = request.EstimatedHours;
            existingPathway.Tags = request.Tags;
            existingPathway.IsOfficial = request.IsOfficial;
            existingPathway.CourseIds = courseIds;
            existingPathway.RoadmapGraphId = graphId;

            await _context.Pathways.ReplaceOneAsync(p => p.Id == id, existingPathway);
            return Ok(existingPathway);
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
        public async Task<IActionResult> UpdateLesson(string id, [FromBody] LessonUpdateDto request)
        {
            var lesson = await _context.Lessons.Find(l => l.Id == id).FirstOrDefaultAsync();
            if (lesson == null) return NotFound();

            lesson.Title = request.Title;
            lesson.Description = request.Description;
            lesson.VideoUrl = request.VideoUrl;
            
            // Update Tasks if provided
            if (request.Tasks != null && request.Tasks.Any())
            {
                foreach (var taskDto in request.Tasks)
                {
                    if (string.IsNullOrEmpty(taskDto.Id))
                    {
                        taskDto.Id = null;
                        await _context.Tasks.InsertOneAsync(taskDto);
                        lesson.TaskIds.Add(taskDto.Id!);
                    }
                    else
                    {
                        await _context.Tasks.ReplaceOneAsync(t => t.Id == taskDto.Id, taskDto);
                        if (!lesson.TaskIds.Contains(taskDto.Id)) lesson.TaskIds.Add(taskDto.Id);
                    }
                }
            }

            await _context.Lessons.ReplaceOneAsync(l => l.Id == id, lesson);
            return Ok(lesson);
        }

        public class LessonUpdateDto : Lesson
        {
            public List<LearningTask>? Tasks { get; set; }
        }

        [HttpDelete("lessons/{id}")]
        public async Task<IActionResult> DeleteLesson(string id)
        {
            await _context.Lessons.DeleteOneAsync(l => l.Id == id);
            return Ok();
        }

        // --- Tasks ---
        [HttpGet("tasks/{id}")]
        public async Task<ActionResult<LearningTask>> GetTaskById(string id) { return Ok(await _context.Tasks.Find(t => t.Id == id).FirstOrDefaultAsync()); }

        [HttpPost("tasks")]
        public async Task<ActionResult<LearningTask>> CreateTask([FromBody] LearningTask task)
        {
            task.Id = null;
            await _context.Tasks.InsertOneAsync(task);
            return Ok(task);
        }

        [HttpPut("tasks/{id}")]
        public async Task<IActionResult> UpdateTask(string id, [FromBody] LearningTask task)
        {
            await _context.Tasks.ReplaceOneAsync(t => t.Id == id, task);
            return Ok();
        }

        [HttpDelete("tasks/{id}")]
        public async Task<IActionResult> DeleteTask(string id)
        {
            await _context.Tasks.DeleteOneAsync(t => t.Id == id);
            return Ok();
        }
    }

    public class AdminResetPasswordRequestDto
    {
        public string NewPassword { get; set; } = string.Empty;
    }
}
