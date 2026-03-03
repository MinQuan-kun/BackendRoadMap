using MongoDB.Driver;
using Microsoft.Extensions.Options;
using BackendService.Configurations;
using BackendService.Models.Entities;

namespace BackendService.Data
{
    public class MongoDbContext
    {
        private readonly IMongoDatabase _database;

        public MongoDbContext(IOptions<DatabaseSettings> settings)
        {
            var client = new MongoClient(settings.Value.GameDevDB);

            _database = client.GetDatabase("GameDevRoadMap");
        }

        public IMongoCollection<User> Users => _database.GetCollection<User>("Users");
        public IMongoCollection<Job> Jobs => _database.GetCollection<Job>("Jobs");
        public IMongoCollection<Roadmap> Roadmaps => _database.GetCollection<Roadmap>("Roadmaps");
        public IMongoCollection<Company> Companies => _database.GetCollection<Company>("Companies");
        public IMongoCollection<Application> Applications => _database.GetCollection<Application>("Applications");
        public IMongoCollection<QuestionBank> Questions => _database.GetCollection<QuestionBank>("QuestionBank");
        public IMongoCollection<Node> Nodes => _database.GetCollection<Node>("Nodes");

    }
}