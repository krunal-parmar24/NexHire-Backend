using System;

namespace NexHire.Domain.Entities
{
    public class Company
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Name { get; set; } = null!;
        public string? Industry { get; set; }
        public string? Size { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
