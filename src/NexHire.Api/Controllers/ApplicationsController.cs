using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.DTOs.Applications;
using NexHire.Application.Exceptions;
using NexHire.Application.Interfaces;
using NexHire.Domain.Entities;
using System.Security.Claims;

namespace NexHire.Api.Controllers
{
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
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var response = await _applicationService.SubmitApplicationAsync(userId, request);
                return StatusCode(201, response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = new { code = "NOT_FOUND", message = ex.Message } });
            }
            catch (ConflictException ex)
            {
                return Conflict(new { error = new { code = ex.Code, message = ex.Message } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = ex.Message } });
            }
        }

        [HttpGet("mine")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> GetMyApplications()
        {
            var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
            var response = await _applicationService.GetMyApplicationsAsync(userId);
            return Ok(new { items = response });
        }

        [HttpPatch("{id:guid}/withdraw")]
        [Authorize(Roles = "JobSeeker")]
        public async Task<IActionResult> WithdrawApplication(Guid id)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                var response = await _applicationService.WithdrawApplicationAsync(userId, id);
                return Ok(response);
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = new { code = "NOT_FOUND", message = ex.Message } });
            }
            catch (ConflictException ex)
            {
                return Conflict(new { error = new { code = ex.Code, message = ex.Message } });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPatch("{id:guid}/status")]
        [Authorize(Roles = "Recruiter")]
        public async Task<IActionResult> UpdateStatus(Guid id, [FromBody] UpdateApplicationStatusRequest request)
        {
            try
            {
                var userId = Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
                await _applicationService.UpdateApplicationStatusAsync(userId, id, request.Status);
                return Ok(new { status = request.Status });
            }
            catch (NotFoundException ex)
            {
                return NotFound(new { error = new { code = "NOT_FOUND", message = ex.Message } });
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new { error = new { code = "VALIDATION_ERROR", message = ex.Message } });
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
