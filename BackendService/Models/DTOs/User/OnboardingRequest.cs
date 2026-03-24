namespace BackendService.Models.DTOs.User
{
    public class OnboardingRequest
    {
        public Dictionary<string, string> Responses { get; set; } = new();
    }
}