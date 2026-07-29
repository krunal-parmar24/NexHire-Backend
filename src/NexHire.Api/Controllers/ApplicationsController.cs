using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Api.Extensions;
using NexHire.Application.DTOs.Applications;
using NexHire.Application.Interfaces;

namespace NexHire.Api.Controllers
{
    /// <summary>Job-seeker application submission/withdrawal and recruiter status-update endpoints.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class ApplicationsController : ControllerBase
    {
        private readonly IApplicationService _applicationService;

        public ApplicationsController(IApplicationService applicationService)
        {
            _applicationService = applicationService;
        }

        [HttpPost]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> SubmitApplication([FromBody] SubmitApplicationRequest request)
        {
            var response = await _applicationService.SubmitApplicationAsync(User.GetUserId(), request);
            return StatusCode(201, response);
        }

        [HttpGet("mine")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> GetMyApplications()
        {
            var response = await _applicationService.GetMyApplicationsAsync(User.GetUserId());
            return Ok(new { items = response });
        }

        [HttpPatch("{id:guid}/withdraw")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> WithdrawApplication(Guid id)
        {
            var response = await _applicationService.WithdrawApplicationAsync(User.GetUserId(), id);
            return Ok(response);
        }

        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateApplicationStatusRequest request)
        {
            await _applicationService.UpdateApplicationStatusAsync(User.GetUserId(), id, request.Status);
            return Ok(new { status = request.Status });
        }
    }
}
