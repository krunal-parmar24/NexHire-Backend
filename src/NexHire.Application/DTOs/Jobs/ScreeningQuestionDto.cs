namespace NexHire.Application.DTOs.Jobs
{
    public class ScreeningQuestionDto
    {
        public string Id { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string Type { get; set; } = null!; // text | single-select | multi-select | file upload | yes/no | numeric
        public bool Required { get; set; }
    }
}
