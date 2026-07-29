using System;
using System.Threading;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Dashboard;
using NexHire.Application.Interfaces;

namespace NexHire.Application.Services
{
    /// <inheritdoc cref="IDashboardService"/>
    public class DashboardService : IDashboardService
    {
        private readonly IJobRepository _jobRepository;
        private readonly IApplicationRepository _applicationRepository;

        public DashboardService(IJobRepository jobRepository, IApplicationRepository applicationRepository)
        {
            _jobRepository = jobRepository;
            _applicationRepository = applicationRepository;
        }

        public async Task<DashboardResponse> GetRecruiterDashboardAsync(Guid recruiterId, CancellationToken cancellationToken = default)
        {
            var company = await _jobRepository.GetCompanyByRecruiterIdAsync(recruiterId);
            var verificationStatus = company?.VerificationStatus.ToString() ?? "Unverified";

            var activeJobsCount = await _jobRepository.GetActiveJobsCountForRecruiterAsync(recruiterId, cancellationToken);
            var (totalApplicants, pendingReview) = await _applicationRepository.GetApplicantCountsForRecruiterAsync(recruiterId, cancellationToken);

            return new DashboardResponse
            {
                ActiveJobPostings = activeJobsCount,
                TotalApplicants = totalApplicants,
                PendingReview = pendingReview,
                VerificationStatus = verificationStatus
            };
        }
    }
}
