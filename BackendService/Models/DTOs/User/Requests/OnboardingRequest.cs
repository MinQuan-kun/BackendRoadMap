namespace BackendService.Models.DTOs.User.Requests
{
    public class OnboardingRequest
    {
        public Dictionary<string, string> Responses { get; set; } = new();
    }
}