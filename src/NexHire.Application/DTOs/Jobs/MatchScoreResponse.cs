using System;

namespace NexHire.Application.DTOs.Jobs
{
    public class PillarScore
    {
        public int Weight { get; set; }
        public int Score { get; set; }
    }

    public class MatchScoreBreakdown
    {
        public PillarScore SkillsCoverage { get; set; } = new();
        public PillarScore ExperienceFit { get; set; } = new();
        public PillarScore CertificationMatch { get; set; } = new();
        public PillarScore DomainTitleMatch { get; set; } = new();
    }

    public class MatchScoreResponse
    {
        public Guid JobId { get; set; }
        public int OverallScore { get; set; }
        public MatchScoreBreakdown Breakdown { get; set; } = new();
        public bool CertificationWeightRedistributed { get; set; }
    }
}
