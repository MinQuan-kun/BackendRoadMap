namespace BackendService.Models.DTOs.Job.Responses
{
    public class JobListResponsedto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string Salary { get; set; } = null!;
        public List<string> Skills { get; set; } = new();
    }
}
