using System.Collections.Generic;

namespace NexHire.Application.DTOs.Onboarding
{
    public class JobSeekerOnboardingRequest
    {
        public string FullName { get; set; } = null!;
        public string Phone { get; set; } = null!;
        public string CurrentTitle { get; set; } = null!;
        public int TotalExperienceYears { get; set; }
        public List<string> Skills { get; set; } = new();
        public string PreferredJobType { get; set; } = null!;
        public string PreferredLocation { get; set; } = null!;
        public List<string> Certifications { get; set; } = new();
        public List<string> PortfolioLinks { get; set; } = new();
        public string? ExpectedSalaryRange { get; set; }
    }
}
