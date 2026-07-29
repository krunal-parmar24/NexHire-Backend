using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Applications;
using NexHire.Application.Exceptions;
using NexHire.Application.Interfaces;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Services
{
    /// <inheritdoc cref="IApplicationService"/>
    public class ApplicationService : IApplicationService
    {
        private readonly IApplicationRepository _applicationRepository;
        private readonly IJobRepository _jobRepository;

        public ApplicationService(IApplicationRepository applicationRepository, IJobRepository jobRepository)
        {
            _applicationRepository = applicationRepository;
            _jobRepository = jobRepository;
        }

        public async Task<SubmitApplicationResponse> SubmitApplicationAsync(Guid userId, SubmitApplicationRequest request, CancellationToken cancellationToken = default)
        {
            var job = await _jobRepository.GetByIdAsync(request.JobId);
            if (job == null)
            {
                throw new NotFoundException("Job not found.");
            }

            var existingApp = await _applicationRepository.GetByJobAndUserAsync(request.JobId, userId, cancellationToken);
            if (existingApp != null)
            {
                throw new ConflictException("DUPLICATE_APPLICATION", "You have already applied to this job.");
            }

            var mandatoryQuestions = job.ScreeningQuestions.Where(q => q.Required).Select(q => q.QuestionId).ToList();
            var providedAnswers = request.Answers.Select(a => a.QuestionId).ToHashSet();

            foreach (var mq in mandatoryQuestions)
            {
                if (!providedAnswers.Contains(mq))
                {
                    throw new ArgumentException($"Missing required answer for question: {mq}");
                }
            }

            if (string.IsNullOrWhiteSpace(request.ResumeUrl))
            {
                throw new ArgumentException("Resume is required.");
            }

            var application = new NexHire.Domain.Entities.Application
            {
                JobId = request.JobId,
                UserId = userId,
                Status = ApplicationStatus.Applied,
                SubmittedAt = DateTime.UtcNow,
                Answers = request.Answers.Select(a => new Answer { QuestionId = a.QuestionId, Value = a.Value }).ToList(),
                ResumeUrl = request.ResumeUrl
            };

            await _applicationRepository.AddAsync(application, cancellationToken);

            return new SubmitApplicationResponse(application.Id, application.Status.ToString());
        }

        public async Task<List<ApplicationDto>> GetMyApplicationsAsync(Guid userId, CancellationToken cancellationToken = default)
        {
            var applications = await _applicationRepository.GetByUserIdAsync(userId, cancellationToken);
            return applications.Select(a => new ApplicationDto(
                a.Id,
                a.JobId,
                a.Job?.Title ?? "Unknown",
                a.Job?.Company?.Name ?? "Unknown",
                a.Status.ToString(),
                a.SubmittedAt
            )).ToList();
        }

        public async Task<WithdrawApplicationResponse> WithdrawApplicationAsync(Guid userId, Guid applicationId, CancellationToken cancellationToken = default)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
            if (application == null)
            {
                throw new NotFoundException("Application not found.");
            }

            if (application.UserId != userId)
            {
                throw new UnauthorizedAccessException("You can only withdraw your own applications.");
            }

            if (application.Status == ApplicationStatus.Hired || application.Status == ApplicationStatus.Rejected)
            {
                throw new ConflictException("WITHDRAWAL_NOT_ALLOWED", "Cannot withdraw after a final decision.");
            }

            application.Status = ApplicationStatus.Withdrawn;
            await _applicationRepository.UpdateAsync(application, cancellationToken);

            return new WithdrawApplicationResponse(application.Status.ToString());
        }

        public async Task<List<ApplicantDto>> GetJobApplicantsAsync(Guid recruiterId, Guid jobId, CancellationToken cancellationToken = default)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
            {
                throw new NotFoundException("JOB_NOT_FOUND", "Job not found.");
            }

            if (job.Company?.RecruiterId != recruiterId)
            {
                throw new UnauthorizedAccessException("You do not have permission to view applicants for this job.");
            }

            var applications = await _applicationRepository.GetByJobIdAsync(jobId, cancellationToken);
            return applications.Select(a => new ApplicantDto(
                a.Id,
                a.User?.Profile?.FullName ?? "Unknown",
                a.Status.ToString(),
                a.Answers.Select(ans => new AnswerDto(ans.QuestionId, ans.Value)).ToList(),
                a.ResumeUrl,
                a.User?.Profile != null ? $"{a.User.Profile.TotalExperienceYears} yrs experience, {a.User.Profile.CurrentTitle}" : null
            )).ToList();
        }

        public async Task UpdateApplicationStatusAsync(Guid recruiterId, Guid applicationId, string status, CancellationToken cancellationToken = default)
        {
            var application = await _applicationRepository.GetByIdAsync(applicationId, cancellationToken);
            if (application == null)
            {
                throw new NotFoundException("Application not found.");
            }

            if (application.Job?.Company?.RecruiterId != recruiterId)
            {
                throw new UnauthorizedAccessException("You do not have permission to edit this application.");
            }

            if (!Enum.TryParse<ApplicationStatus>(status, true, out var parsedStatus))
            {
                throw new ArgumentException("Invalid application status.");
            }

            if (parsedStatus == ApplicationStatus.Withdrawn)
            {
                throw new ArgumentException("Cannot set status to Withdrawn.");
            }

            application.Status = parsedStatus;
            await _applicationRepository.UpdateAsync(application, cancellationToken);
        }
    }
}
