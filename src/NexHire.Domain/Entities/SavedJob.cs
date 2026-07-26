using System;

namespace NexHire.Domain.Entities
{
    public class SavedJob
    {
        public Guid UserId { get; set; }
        public Guid JobId { get; set; }
        public DateTime SavedAt { get; set; } = DateTime.UtcNow;

        // Navigation properties
        public User? User { get; set; }
        public Job? Job { get; set; }
    }
}
