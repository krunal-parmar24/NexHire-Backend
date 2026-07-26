using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.DTOs.Jobs;
using NexHire.Application.Interfaces;

namespace NexHire.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : ControllerBase
    {
        private readonly IJobService _jobService;

        public JobsController(IJobService jobService)
        {
            _jobService = jobService;
        }

        [HttpGet]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobs(
            [FromQuery] string? keyword,
            [FromQuery] string? location,
            [FromQuery] string? jobType,
            [FromQuery] string? remoteType,
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var result = await _jobService.GetJobsAsync(keyword, location, jobType, remoteType, page, pageSize);
            return Ok(result);
        }

        [HttpGet("{id}")]
        [AllowAnonymous]
        public async Task<IActionResult> GetJobById(Guid id)
        {
            var job = await _jobService.GetJobByIdAsync(id);
            if (job == null)
            {
                return NotFound(new { error = new { code = "JOB_NOT_FOUND", message = "Job listing not found." } });
            }

            // Guests/Seekers should only be able to view Active jobs
            // Recruiters can view their own jobs regardless of status
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            var isOwner = !string.IsNullOrEmpty(userIdStr) && 
                          Guid.TryParse(userIdStr, out var userId) && 
                          role == "Recruiter" && 
                          job.RecruiterId == userId;

            if (job.Status != "Active" && !isOwner)
            {
                return NotFound(new { error = new { code = "JOB_NOT_FOUND", message = "Job listing not found." } });
            }

            return Ok(job);
        }

        [HttpGet("saved")]
        [Authorize]
        public async Task<IActionResult> GetSavedJobs()
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var savedJobIds = await _jobService.GetSavedJobIdsAsync(userId);
            return Ok(savedJobIds);
        }

        [HttpPost("{id}/save")]
        [Authorize]
        public async Task<IActionResult> ToggleSavedJob(Guid id)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var role = User.FindFirst(System.Security.Claims.ClaimTypes.Role)?.Value;
            if (role == "Recruiter")
            {
                return Forbid();
            }

            var isSaved = await _jobService.ToggleSavedJobAsync(userId, id);
            return Ok(new { isSaved });
        }

        [HttpPost]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobRequest request)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var result = await _jobService.CreateJobAsync(request, userId);
            return CreatedAtAction(nameof(GetJobById), new { id = result.Id }, result);
        }

        [HttpPut("{id}")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateJob(Guid id, [FromBody] CreateJobRequest request)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var result = await _jobService.UpdateJobAsync(id, request, userId);
            if (result == null)
            {
                return NotFound(new { error = new { code = "JOB_NOT_FOUND", message = "Job listing not found." } });
            }

            return Ok(result);
        }

        [HttpPatch("{id}/status")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateJobStatus(Guid id, [FromBody] UpdateJobStatusRequest request)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var result = await _jobService.UpdateJobStatusAsync(id, request.Status, userId);
            if (result == null)
            {
                return NotFound(new { error = new { code = "JOB_NOT_FOUND", message = "Job listing not found." } });
            }

            return Ok(result);
        }

        [HttpGet("mine")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> GetMyJobs(
            [FromQuery] int page = 1,
            [FromQuery] int pageSize = 20)
        {
            var userIdStr = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var userId))
                return Unauthorized();

            var result = await _jobService.GetJobsByRecruiterAsync(userId, page, pageSize);
            return Ok(result);
        }
    }
}
