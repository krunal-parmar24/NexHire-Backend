using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Infrastructure.Persistence.Repositories
{
    public class JobRepository : IJobRepository
    {
        private readonly NexHireDbContext _db;

        public JobRepository(NexHireDbContext db)
        {
            _db = db;
        }

        public async Task<Job?> GetByIdAsync(Guid id)
        {
            return await _db.Jobs
                .Include(j => j.Company)
                .FirstOrDefaultAsync(j => j.Id == id);
        }

        public async Task<(List<Job> Items, int TotalCount)> GetJobsAsync(
            string? keyword,
            string? location,
            string? jobType,
            string? remoteType,
            int page,
            int pageSize)
        {
            var query = _db.Jobs
                .Include(j => j.Company)
                .Where(j => j.Status == JobStatus.Active)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(keyword))
            {
                var lowerKeyword = keyword.ToLower();
                query = query.Where(j => j.Title.ToLower().Contains(lowerKeyword) || j.Description.ToLower().Contains(lowerKeyword));
            }

            if (!string.IsNullOrWhiteSpace(location))
            {
                var lowerLocation = location.ToLower();
                query = query.Where(j => j.Location.ToLower().Contains(lowerLocation));
            }

            if (!string.IsNullOrWhiteSpace(jobType))
            {
                var lowerJobType = jobType.ToLower();
                query = query.Where(j => j.JobType.ToLower() == lowerJobType);
            }

            if (!string.IsNullOrWhiteSpace(remoteType))
            {
                var lowerRemoteType = remoteType.ToLower();
                query = query.Where(j => j.RemoteType.ToLower() == lowerRemoteType);
            }

            var totalCount = await query.CountAsync();
            
            var items = await query
                .OrderByDescending(j => j.CreatedAt)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return (items, totalCount);
        }

        public async Task<List<Guid>> GetSavedJobIdsAsync(Guid userId)
        {
            return await _db.SavedJobs
                .Where(sj => sj.UserId == userId)
                .Select(sj => sj.JobId)
                .ToListAsync();
        }

        public async Task<bool> ToggleSavedJobAsync(Guid userId, Guid jobId)
        {
            var savedJob = await _db.SavedJobs
                .FirstOrDefaultAsync(sj => sj.UserId == userId && sj.JobId == jobId);

            if (savedJob != null)
            {
                _db.SavedJobs.Remove(savedJob);
                await _db.SaveChangesAsync();
                return false; // Unsaved
            }
            else
            {
                var newSavedJob = new SavedJob
                {
                    UserId = userId,
                    JobId = jobId,
                    SavedAt = DateTime.UtcNow
                };
                _db.SavedJobs.Add(newSavedJob);
                await _db.SaveChangesAsync();
                return true; // Saved
            }
        }
    }
}
