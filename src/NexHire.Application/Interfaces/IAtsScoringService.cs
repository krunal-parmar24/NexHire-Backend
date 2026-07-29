using System;
using System.Threading;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Jobs;

namespace NexHire.Application.Interfaces
{
    /// <summary>
    /// Computes the 4-pillar (skills, experience, certification, domain/title) weighted ATS match score
    /// between a job seeker's profile and a job listing.
    /// </summary>
    public interface IAtsScoringService
    {
        /// <summary>Computes the match score breakdown for a given job and job-seeker user.</summary>
        Task<MatchScoreResponse> GetMatchScoreAsync(Guid jobId, Guid userId, CancellationToken ct);
    }
}
