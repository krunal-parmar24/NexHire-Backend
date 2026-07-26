using System;
using System.Collections.Generic;

namespace NexHire.Application.DTOs.Jobs
{
    public class JobDetailResponse
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string JobType { get; set; } = null!;
        public string? SalaryRange { get; set; }
        public string RemoteType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public List<ScreeningQuestionDto> ScreeningQuestions { get; set; } = new();
    }
}
