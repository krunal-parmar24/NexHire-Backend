using System;

namespace NexHire.Application.DTOs.Jobs
{
    public class JobListItemDto
    {
        public Guid Id { get; set; }
        public string Title { get; set; } = null!;
        public string CompanyName { get; set; } = null!;
        public string Location { get; set; } = null!;
        public string JobType { get; set; } = null!;
        public string RemoteType { get; set; } = null!;
        public string Status { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
    }
}
