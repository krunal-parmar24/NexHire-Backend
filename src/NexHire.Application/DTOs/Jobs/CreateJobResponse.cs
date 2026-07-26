using System;

namespace NexHire.Application.DTOs.Jobs
{
    public class CreateJobResponse
    {
        public Guid Id { get; set; }
        public string Status { get; set; } = null!;
    }
}
