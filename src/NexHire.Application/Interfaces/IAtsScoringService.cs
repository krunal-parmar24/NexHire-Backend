using System;
using System.Threading;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Jobs;

namespace NexHire.Application.Interfaces
{
    public interface IAtsScoringService
    {
        Task<MatchScoreResponse> GetMatchScoreAsync(Guid jobId, Guid userId, CancellationToken ct);
    }
}
