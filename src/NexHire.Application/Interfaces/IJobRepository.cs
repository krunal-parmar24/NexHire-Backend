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
    }
}
