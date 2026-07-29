using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Jobs;
using NexHire.Application.Interfaces;

namespace NexHire.Application.Services
{
    /// <inheritdoc cref="IJobService"/>
    public class JobService : IJobService
    {
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<JobDetailResponse?> GetJobByIdAsync(Guid id)
        {
            var job = await _jobRepository.GetByIdAsync(id);
            if (job == null) return null;

            return new JobDetailResponse
            {
                Id = job.Id,
                Title = job.Title,
                CompanyName = job.Company?.Name ?? "Unknown Company",
                RecruiterId = job.Company?.RecruiterId ?? Guid.Empty,
                Description = job.Description,
                Requirements = job.Requirements,
                Location = job.Location,
                JobType = job.JobType,
                SalaryRange = job.SalaryRange,
                RemoteType = job.RemoteType,
                Status = job.Status.ToString(),
                ScreeningQuestions = job.ScreeningQuestions.Select(sq => new ScreeningQuestionDto
                {
                    Id = sq.QuestionId,
                    Label = sq.Label,
                    Type = sq.Type,
                    Required = sq.Required,
                    Options = sq.Options
                }).ToList()
            };
        }

        public async Task<JobListResponse> GetJobsAsync(
            string? keyword,
            string? location,
            string? jobType,
            string? remoteType,
            int page,
            int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var (items, totalCount) = await _jobRepository.GetJobsAsync(keyword, location, jobType, remoteType, page, pageSize);

            return new JobListResponse
            {
                Items = items.Select(job => new JobListItemDto
                {
                    Id = job.Id,
                    Title = job.Title,
                    CompanyName = job.Company?.Name ?? "Unknown Company",
                    Location = job.Location,
                    JobType = job.JobType,
                    RemoteType = job.RemoteType,
                    Status = job.Status.ToString(),
                    CreatedAt = job.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }

        public async Task<List<Guid>> GetSavedJobIdsAsync(Guid userId)
        {
            return await _jobRepository.GetSavedJobIdsAsync(userId);
        }

        public async Task<bool> ToggleSavedJobAsync(Guid userId, Guid jobId)
        {
            return await _jobRepository.ToggleSavedJobAsync(userId, jobId);
        }

        public async Task<CreateJobResponse> CreateJobAsync(CreateJobRequest request, Guid recruiterId)
        {
            var company = await _jobRepository.GetCompanyByRecruiterIdAsync(recruiterId);
            if (company == null)
            {
                throw new UnauthorizedAccessException("Recruiter company not found.");
            }

            var job = new NexHire.Domain.Entities.Job
            {
                CompanyId = company.Id,
                Title = request.Title,
                Description = request.Description,
                Requirements = request.Requirements,
                Location = request.Location,
                JobType = request.JobType,
                SalaryRange = request.SalaryRange,
                RemoteType = request.RemoteType,
                Status = NexHire.Domain.Enums.JobStatus.Active,
                ScreeningQuestions = request.ScreeningQuestions.Select(sq => new NexHire.Domain.Entities.ScreeningQuestion
                {
                    QuestionId = sq.Id,
                    Label = sq.Label,
                    Type = sq.Type,
                    Required = sq.Required,
                    Options = sq.Options
                }).ToList()
            };

            var created = await _jobRepository.CreateAsync(job);

            return new CreateJobResponse
            {
                Id = created.Id,
                Status = created.Status.ToString()
            };
        }

        public async Task<JobDetailResponse?> UpdateJobAsync(Guid jobId, CreateJobRequest request, Guid recruiterId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return null;

            if (job.Company?.RecruiterId != recruiterId)
            {
                throw new UnauthorizedAccessException("You do not have permission to edit this job.");
            }

            job.Title = request.Title;
            job.Description = request.Description;
            job.Requirements = request.Requirements;
            job.Location = request.Location;
            job.JobType = request.JobType;
            job.SalaryRange = request.SalaryRange;
            job.RemoteType = request.RemoteType;
            job.ScreeningQuestions = request.ScreeningQuestions.Select(sq => new NexHire.Domain.Entities.ScreeningQuestion
            {
                QuestionId = sq.Id,
                Label = sq.Label,
                Type = sq.Type,
                Required = sq.Required,
                Options = sq.Options
            }).ToList();

            await _jobRepository.UpdateAsync(job);

            return await GetJobByIdAsync(jobId);
        }

        public async Task<JobDetailResponse?> UpdateJobStatusAsync(Guid jobId, string status, Guid recruiterId)
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null) return null;

            if (job.Company?.RecruiterId != recruiterId)
            {
                throw new UnauthorizedAccessException("You do not have permission to edit this job.");
            }

            if (!Enum.TryParse<NexHire.Domain.Enums.JobStatus>(status, true, out var parsedStatus))
            {
                throw new ArgumentException("Invalid job status.");
            }

            job.Status = parsedStatus;
            await _jobRepository.UpdateAsync(job);

            return await GetJobByIdAsync(jobId);
        }

        public async Task<JobListResponse> GetJobsByRecruiterAsync(Guid recruiterId, int page, int pageSize)
        {
            if (page <= 0) page = 1;
            if (pageSize <= 0) pageSize = 20;

            var (items, totalCount) = await _jobRepository.GetJobsByRecruiterAsync(recruiterId, page, pageSize);

            return new JobListResponse
            {
                Items = items.Select(job => new JobListItemDto
                {
                    Id = job.Id,
                    Title = job.Title,
                    CompanyName = job.Company?.Name ?? "Unknown Company",
                    Location = job.Location,
                    JobType = job.JobType,
                    RemoteType = job.RemoteType,
                    Status = job.Status.ToString(),
                    CreatedAt = job.CreatedAt
                }).ToList(),
                TotalCount = totalCount,
                Page = page,
                PageSize = pageSize
            };
        }
    }
}
