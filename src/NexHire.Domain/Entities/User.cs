using System;
using NexHire.Domain.Enums;

namespace NexHire.Domain.Entities
{
    public class User
    {
        public Guid Id { get; set; } = Guid.NewGuid();
        public string Email { get; set; } = null!;
        public string PasswordHash { get; set; } = null!;
        public UserRole Role { get; set; }
        public bool OnboardingCompleted { get; set; } = false;
        public int CreditBalance { get; set; } = 500;
        public DateTime? CreditResetDate { get; set; }
        public UserProfile? Profile { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}
