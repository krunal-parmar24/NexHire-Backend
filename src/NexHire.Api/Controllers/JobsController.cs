using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
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
            // (Recruiter ownership/draft viewing will be implemented in Day 4)
            if (job.Status != "Active")
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
    }
}
