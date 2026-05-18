namespace BackendService.Models.DTOs.Assessment
{
    public class ActiveQuizDto
    {
        public string? Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string? Description { get; set; }
        public List<QuizQuestionDto> Questions { get; set; } = new();
    }

    public class QuizQuestionDto
    {
        public string? Id { get; set; }
        public string Question { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public int Order { get; set; }
        public List<QuizOptionDto> Options { get; set; } = new();
    }

    public class QuizOptionDto
    {
        public string Text { get; set; } = string.Empty;
    }

    public class QuizSubmissionDto
    {
        public string QuizId { get; set; } = string.Empty;
        public Dictionary<string, string> Answers { get; set; } = new();
    }
}
