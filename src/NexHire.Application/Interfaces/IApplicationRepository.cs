using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace NexHire.Application.Interfaces
{
    public interface IApplicationRepository
    {
        Task<NexHire.Domain.Entities.Application?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);
        Task<NexHire.Domain.Entities.Application?> GetByJobAndUserAsync(Guid jobId, Guid userId, CancellationToken cancellationToken = default);
        Task<List<NexHire.Domain.Entities.Application>> GetByUserIdAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<List<NexHire.Domain.Entities.Application>> GetByJobIdAsync(Guid jobId, CancellationToken cancellationToken = default);
        Task AddAsync(NexHire.Domain.Entities.Application application, CancellationToken cancellationToken = default);
        Task UpdateAsync(NexHire.Domain.Entities.Application application, CancellationToken cancellationToken = default);
    }
}
