using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NexHire.Application.Interfaces
{
    /// <summary>
    /// Persistence abstraction for job applications and their recruiter/seeker-facing aggregates.
    /// </summary>
    public interface IApplicationRepository
    {
        /// <summary>Gets an application by id, including its job/company and user/profile navigation properties.</summary>
        Task<NexHire.Domain.Entities.Application?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

        /// <summary>Gets the existing application (if any) for a given job and user, used for duplicate-application checks.</summary>
        Task<NexHire.Domain.Entities.Application?> GetByJobAndUserAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default);

        /// <summary>Gets all applications submitted by a given user, ordered by most recent first.</summary>
        Task<List<NexHire.Domain.Entities.Application>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>Gets all applications submitted for a given job, ordered by most recent first.</summary>
        Task<List<NexHire.Domain.Entities.Application>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);

        /// <summary>Persists a new application.</summary>
        Task AddAsync(NexHire.Domain.Entities.Application application, CancellationToken cancellationToken = default);

        /// <summary>Persists changes to an existing application.</summary>
        Task UpdateAsync(NexHire.Domain.Entities.Application application, CancellationToken cancellationToken = default);

        /// <summary>Gets the total applicant count and pending-review count across all jobs owned by a recruiter.</summary>
        Task<(int TotalApplicants, int PendingReview)> GetApplicantCountsForRecruiterAsync(Guid recruiterId, CancellationToken cancellationToken = default);
    }
}
