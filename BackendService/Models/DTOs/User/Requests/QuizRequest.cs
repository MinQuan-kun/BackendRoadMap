namespace BackendService.Models.DTOs.User.Requests
{
    public class QuizRequest
    {
        public Dictionary<string, string> Responses { get; set; } = new();
    }
}
