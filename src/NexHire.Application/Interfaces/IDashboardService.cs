using System;
using System.Threading;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Dashboard;

namespace NexHire.Application.Interfaces
{
    /// <summary>Aggregates recruiter-facing dashboard metrics (active postings, applicant counts, verification status).</summary>
    public interface IDashboardService
    {
        /// <summary>Builds the recruiter dashboard summary for the given recruiter.</summary>
        Task<DashboardResponse> GetRecruiterDashboardAsync(Guid recruiterId, CancellationToken cancellationToken = default);
    }
}
