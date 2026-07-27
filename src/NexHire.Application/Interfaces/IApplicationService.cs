using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Applications;

namespace NexHire.Application.Interfaces
{
    public interface IApplicationService
    {
        Task<SubmitApplicationResponse> SubmitApplicationAsync(Guid userId, SubmitApplicationRequest request, CancellationToken cancellationToken = default);
        Task<List<ApplicationDto>> GetMyApplicationsAsync(Guid userId, CancellationToken cancellationToken = default);
        Task<WithdrawApplicationResponse> WithdrawApplicationAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);
        Task<List<ApplicantDto>> GetJobApplicantsAsync(Guid recruiterId, Guid jobId, CancellationToken cancellationToken = default);
        Task UpdateApplicationStatusAsync(Guid recruiterId, Guid applicationId, string status, CancellationToken cancellationToken = default);
    }
}
