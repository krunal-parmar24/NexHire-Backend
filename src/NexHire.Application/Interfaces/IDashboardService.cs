using System;
using System.Threading;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Dashboard;

namespace NexHire.Application.Interfaces
{
    public interface IDashboardService
    {
        Task<DashboardResponse> GetRecruiterDashboardAsync(Guid recruiterId, CancellationToken cancellationToken = default);
    }
}
