using System.Collections.Generic;

namespace NexHire.Application.DTOs.Onboarding
{
    public class ParsedFieldsDto
    {
        public string? FullName { get; set; }
        public string? Phone { get; set; }
        public string? CurrentTitle { get; set; }
        public int? TotalExperienceYears { get; set; }
        public List<string> Skills { get; set; } = new();
        public string? PreferredJobType { get; set; }
        public string? PreferredLocation { get; set; }
        public List<string> Certifications { get; set; } = new();
        public List<string> PortfolioLinks { get; set; } = new();
        public string? ExpectedSalaryRange { get; set; }
    }

    public class ParseResumeResponse
    {
        public ParsedFieldsDto ParsedFields { get; set; } = new();
        public int CreditsDeducted { get; set; } = 0;
    }
}
