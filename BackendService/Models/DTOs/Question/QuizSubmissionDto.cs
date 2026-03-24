namespace BackendService.Models.DTOs.Question
{
    public class QuizSubmissionDto
    {
        public string UserId { get; set; } = null!;
        public List<string> SelectedNodeIds { get; set; } = new();

        public bool SkipBasics { get; set; }
    }
}
