using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces
{
    /// <summary>Persistence abstraction for job listings, saved jobs, and recruiter company lookups.</summary>
    public interface IJobRepository
    {
        /// <summary>Gets a job by id, including its company navigation property.</summary>
        Task<Job?> GetByIdAsync(Guid id);

        /// <summary>Gets a paged, filtered list of active job listings.</summary>
        Task<(List<Job> Items, int TotalCount)> GetJobsAsync(string? keyword, string? location, string? jobType, string? remoteType, int page, int pageSize);

        /// <summary>Gets the ids of all jobs saved by the given user.</summary>
        Task<List<Guid>> GetSavedJobIdsAsync(Guid userId);

        /// <summary>Toggles whether a job is saved by the given user; returns the resulting saved state.</summary>
        Task<bool> ToggleSavedJobAsync(Guid userId, Guid jobId);

        /// <summary>Gets the company owned by the given recruiter, if any.</summary>
        Task<Company?> GetCompanyByRecruiterIdAsync(Guid recruiterId);

        /// <summary>Persists a new job listing.</summary>
        Task<Job> CreateAsync(Job job);

        /// <summary>Persists changes to an existing job listing.</summary>
        Task UpdateAsync(Job job);

        /// <summary>Gets a paged list of job listings owned by the given recruiter, regardless of status.</summary>
        Task<(List<Job> Items, int TotalCount)> GetJobsByRecruiterAsync(Guid recruiterId, int page, int pageSize);

        /// <summary>Gets the count of active job listings owned by the given recruiter.</summary>
        Task<int> GetActiveJobsCountForRecruiterAsync(Guid recruiterId, CancellationToken cancellationToken = default);
    }
}
