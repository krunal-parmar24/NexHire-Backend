using System;

namespace NexHire.Domain.Entities
{
    public class Company
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string? Industry { get; set; }
        public string? Size { get; set; }
        public Guid RecruiterId { get; set; }
        public Enums.VerificationStatus VerificationStatus { get; set; } = Enums.VerificationStatus.Unverified;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
