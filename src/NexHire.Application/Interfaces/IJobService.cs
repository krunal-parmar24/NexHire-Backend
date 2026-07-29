using System;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Jobs;

namespace NexHire.Application.Interfaces
{
    /// <summary>Application-layer orchestration for job listing CRUD, search, and saved-jobs behavior.</summary>
    public interface IJobService
    {
        /// <summary>Gets full job details by id, or <c>null</c> if not found.</summary>
        Task<JobDetailResponse?> GetJobByIdAsync(Guid id);

        /// <summary>Gets a paged, filtered list of active job listings for guest/seeker browsing.</summary>
        Task<JobListResponse> GetJobsAsync(string? keyword, string? location, string? jobType, string? remoteType, int page, int pageSize);

        /// <summary>Gets the ids of all jobs saved by the given user.</summary>
        Task<System.Collections.Generic.List<Guid>> GetSavedJobIdsAsync(Guid userId);

        /// <summary>Toggles whether a job is saved by the given user; returns the resulting saved state.</summary>
        Task<bool> ToggleSavedJobAsync(Guid userId, Guid jobId);

        /// <summary>Creates a new job listing under the recruiter's company.</summary>
        Task<CreateJobResponse> CreateJobAsync(CreateJobRequest request, Guid recruiterId);

        /// <summary>Updates an existing job listing owned by the given recruiter.</summary>
        Task<JobDetailResponse?> UpdateJobAsync(Guid jobId, CreateJobRequest request, Guid recruiterId);

        /// <summary>Updates the status of a job listing owned by the given recruiter.</summary>
        Task<JobDetailResponse?> UpdateJobStatusAsync(Guid jobId, string status, Guid recruiterId);

        /// <summary>Gets a paged list of job listings owned by the given recruiter, regardless of status.</summary>
        Task<JobListResponse> GetJobsByRecruiterAsync(Guid recruiterId, int page, int pageSize);
    }
}
