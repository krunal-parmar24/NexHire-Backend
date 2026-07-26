using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces
{
    public interface IJobRepository
    {
        Task<Job?> GetByIdAsync(Guid id);
        Task<(List<Job> Items, int TotalCount)> GetJobsAsync(string? keyword, string? location, string? jobType, string? remoteType, int page, int pageSize);
        Task<List<Guid>> GetSavedJobIdsAsync(Guid userId);
        Task<bool> ToggleSavedJobAsync(Guid userId, Guid jobId);
        Task<Company?> GetCompanyByRecruiterIdAsync(Guid recruiterId);
        Task<Job> CreateAsync(Job job);
        Task UpdateAsync(Job job);
        Task<(List<Job> Items, int TotalCount)> GetJobsByRecruiterAsync(Guid recruiterId, int page, int pageSize);
    }
}
