using System;
using System.Collections.Generic;
using NexHire.Domain.Enums;

namespace NexHire.Domain.Entities
{
    public class Job
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid CompanyId { get; set; }
        public Company Company { get; set; } = null!;
        public string Title { get; set; } = null!;
        public string Description { get; set; } = null!;
        public string Requirements { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string JobType { get; set; } = null!; // Full-time, Part-time, Contract
        public string? SalaryRange { get; set; }
        public string RemoteType { get; set; } = null!; // Remote, Hybrid, Onsite
        public JobStatus Status { get; set; } = JobStatus.Draft;
        public List<ScreeningQuestion> ScreeningQuestions { get; set; } = new();
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
