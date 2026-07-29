using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Applications;

namespace NexHire.Application.Interfaces
{
    /// <summary>
    /// Application-layer orchestration for job seeker application submission/withdrawal and
    /// recruiter-side applicant review.
    /// </summary>
    public interface IApplicationService
    {
        /// <summary>Validates screening-question completeness and submits a new application for a job.</summary>
        Task<SubmitApplicationResponse> SubmitApplicationAsync(Guid userId, SubmitApplicationRequest request, CancellationToken cancellationToken = default);

        /// <summary>Gets all applications submitted by the given job seeker.</summary>
        Task<List<ApplicationDto>> GetMyApplicationsAsync(Guid userId, CancellationToken cancellationToken = default);

        /// <summary>Withdraws a job seeker's own application, provided it has not reached a final decision.</summary>
        Task<WithdrawApplicationResponse> WithdrawApplicationAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default);

        /// <summary>Gets all applicants for a job owned by the given recruiter.</summary>
        Task<List<ApplicantDto>> GetJobApplicantsAsync(Guid recruiterId, Guid jobId, CancellationToken cancellationToken = default);

        /// <summary>Updates the status of an application on a job owned by the given recruiter.</summary>
        Task UpdateApplicationStatusAsync(Guid recruiterId, Guid applicationId, string status, CancellationToken cancellationToken = default);
    }
}
