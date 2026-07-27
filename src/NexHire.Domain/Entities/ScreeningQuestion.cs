using System.Collections.Generic;

namespace NexHire.Domain.Entities
{
    public class ScreeningQuestion
    {
        public string QuestionId { get; set; } = null!;
        public string Label { get; set; } = null!;
        public string Type { get; set; } = null!; // text | single-select | multi-select | file upload | yes/no | numeric
        public bool Required { get; set; }
        public List<string>? Options { get; set; }
    }
}
