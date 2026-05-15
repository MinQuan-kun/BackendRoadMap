using BackendService.Configurations;
using BackendService.Models.Entities;
using BackendService.Models.Entities.Recruitment;
using Microsoft.Extensions.Options;
using MongoDB.Bson;
using MongoDB.Driver;

namespace BackendService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<DatabaseSettings> settings)
        {
            var client = new MongoClient(settings.Value.GameDevDB);
            _database = client.GetDatabase("GameDevRoadmapDB");

            try
            {
                _database.RunCommand<BsonDocument>(new BsonDocument("ping", 1));
                Console.WriteLine("Kết nối thành công tới Database: " + _database.DatabaseNamespace.DatabaseName);
            }
            catch (Exception ex)
            {
                Console.WriteLine("Kết nối thất bại: " + ex.Message);
            }
        }

        public IMongoCollection<T> GetCollection<T>(string name)
        {
            return _database.GetCollection<T>(name);
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");

        // Learning Collection
        public IMongoCollection<Pathway> Pathways => _database.GetCollection<Pathway>("Pathways");
        public IMongoCollection<Course> Courses => _database.GetCollection<Course>("Courses");
        public IMongoCollection<Module> Modules => _database.GetCollection<Module>("Modules");
        public IMongoCollection<Lesson> Lessons => _database.GetCollection<Lesson>("Lessons");
        public IMongoCollection<LearningTask> Tasks => _database.GetCollection<LearningTask>("Tasks");

        // Roadmap Collection
        public IMongoCollection<RoadmapGraph> RoadmapGraphs => _database.GetCollection<RoadmapGraph>("RoadmapGraphs");
        public IMongoCollection<RoadmapNode> RoadmapNodes => _database.GetCollection<RoadmapNode>("RoadmapNodes");
        public IMongoCollection<RoadmapEdge> RoadmapEdges => _database.GetCollection<RoadmapEdge>("RoadmapEdges");

        // Assessment Collection
        public IMongoCollection<CareerQuiz> CareerQuizzes => _database.GetCollection<CareerQuiz>("CareerQuizzes");
        public IMongoCollection<CareerQuestion> CareerQuestions => _database.GetCollection<CareerQuestion>("CareerQuestions");
        public IMongoCollection<CareerQuizResult> CareerQuizResults => _database.GetCollection<CareerQuizResult>("CareerQuizResults");

        // Progression Collection
        public IMongoCollection<UserProgress> UserProgress => _database.GetCollection<UserProgress>("UserProgress");

        // Career Collection
        public IMongoCollection<Job> Jobs => _database.GetCollection<Job>("Job");
        public IMongoCollection<JobApplication> JobApplications => _database.GetCollection<JobApplication>("JobApplication");

    }
}