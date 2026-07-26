using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Jobs;
using NexHire.Application.Interfaces;

namespace NexHire.Application.Services
{
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
                    Required = sq.Required
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
    }
}
