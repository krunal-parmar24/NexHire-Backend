using System;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Jobs;

namespace NexHire.Application.Interfaces
{
    public interface IJobService
    {
        Task<JobDetailResponse?> GetJobByIdAsync(Guid id);
        Task<JobListResponse> GetJobsAsync(string? keyword, string? location, string? jobType, string? remoteType, int page, int pageSize);
        Task<System.Collections.Generic.List<Guid>> GetSavedJobIdsAsync(Guid userId);
        Task<bool> ToggleSavedJobAsync(Guid userId, Guid jobId);
        Task<CreateJobResponse> CreateJobAsync(CreateJobRequest request, Guid recruiterId);
        Task<JobDetailResponse?> UpdateJobAsync(Guid jobId, CreateJobRequest request, Guid recruiterId);
        Task<JobDetailResponse?> UpdateJobStatusAsync(Guid jobId, string status, Guid recruiterId);
        Task<JobListResponse> GetJobsByRecruiterAsync(Guid recruiterId, int page, int pageSize);
    }
}
