using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces;

namespace NexHire.Infrastructure.Persistence.Repositories
{
    /// <inheritdoc cref="IApplicationRepository"/>
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly NexHireDbContext _dbContext;

        public ApplicationRepository(NexHireDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public async Task<NexHire.Domain.Entities.Application?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Applications
                .Include(a => a.Job)
                .ThenInclude(j => j.Company)
                .Include(a => a.User)
                .ThenInclude(u => u.Profile)
                .FirstOrDefaultAsync(a => a.Id == id, cancellationToken);
        }

        public async Task<NexHire.Domain.Entities.Application?> GetByJobAndUserAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Applications
                .FirstOrDefaultAsync(a => a.JobId == jobId && a.UserId == userId, cancellationToken);
        }

        public async Task<List<NexHire.Domain.Entities.Application>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Applications
                .Include(a => a.Job)
                .ThenInclude(j => j.Company)
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<NexHire.Domain.Entities.Application>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Applications
                .Include(a => a.User)
                .ThenInclude(u => u.Profile)
                .Where(a => a.JobId == jobId)
                .OrderByDescending(a => a.SubmittedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task AddAsync(NexHire.Domain.Entities.Application application, CancellationToken cancellationToken = default)
        {
            await _dbContext.Applications.AddAsync(application, cancellationToken);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task UpdateAsync(NexHire.Domain.Entities.Application application, CancellationToken cancellationToken = default)
        {
            _dbContext.Applications.Update(application);
            await _dbContext.SaveChangesAsync(cancellationToken);
        }

        public async Task<(int TotalApplicants, int PendingReview)> GetApplicantCountsForRecruiterAsync(Guid recruiterId, CancellationToken cancellationToken = default)
        {
            var query = _dbContext.Applications
                .Where(a => a.Job != null && a.Job.Company != null && a.Job.Company.RecruiterId == recruiterId);

            var totalApplicants = await query.CountAsync(cancellationToken);
            var pendingReview = await query.CountAsync(a => a.Status == NexHire.Domain.Enums.ApplicationStatus.Applied, cancellationToken);

            return (totalApplicants, pendingReview);
        }
    }
}
