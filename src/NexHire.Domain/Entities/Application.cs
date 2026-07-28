using System;
using System.Collections.Generic;
using NexHire.Domain.Enums;

namespace NexHire.Domain.Entities
{
    public class Application
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public Guid JobId { get; set; }
        public Job Job { get; set; } = null!;
        public Guid UserId { get; set; }
        public User User { get; set; } = null!;
        public List<Answer> Answers { get; set; } = new();
        public ApplicationStatus Status { get; set; } = ApplicationStatus.Applied;
        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;
        public string? ResumeUrl { get; set; }
    }
}
