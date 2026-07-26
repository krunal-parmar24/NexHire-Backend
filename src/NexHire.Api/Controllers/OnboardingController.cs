using System;
using System.Security.Claims;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.DTOs.Onboarding;
using NexHire.Application.Interfaces;

namespace NexHire.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class OnboardingController : ControllerBase
    {
        private readonly IOnboardingService _onboardingService;
        private readonly IResumeParsingService _resumeParsingService;

        public OnboardingController(IOnboardingService onboardingService, IResumeParsingService resumeParsingService)
        {
            _onboardingService = onboardingService;
            _resumeParsingService = resumeParsingService;
        }

        private Guid GetUserId()
        {
            return Guid.Parse(User.FindFirst(ClaimTypes.NameIdentifier)!.Value);
        }

        [HttpPost("jobseeker")]
        public async Task<IActionResult> JobSeeker([FromBody] JobSeekerOnboardingRequest req)
        {
            var success = await _onboardingService.CompleteJobSeekerOnboardingAsync(GetUserId(), req);
            if (!success) return BadRequest(new { error = new { code = "INVALID_ROLE", message = "User is not a JobSeeker or not found" } });
            
            return Ok(new { onboardingCompleted = true });
        }

        [HttpPost("recruiter")]
        public async Task<IActionResult> Recruiter([FromBody] RecruiterOnboardingRequest req)
        {
            var success = await _onboardingService.CompleteRecruiterOnboardingAsync(GetUserId(), req);
            if (!success) return BadRequest(new { error = new { code = "INVALID_ROLE", message = "User is not a Recruiter or not found" } });

            return Ok(new { onboardingCompleted = true, verificationStatus = "Unverified" });
        }

        [HttpPost("parse-resume")]
        public async Task<IActionResult> ParseResume(IFormFile file)
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = new { code = "INVALID_FILE", message = "No file uploaded" } });

            if (file.Length > 1 * 1024 * 1024) // 1MB limit
                return BadRequest(new { error = new { code = "FILE_TOO_LARGE", message = "File exceeds 1MB limit" } });

            using var stream = file.OpenReadStream();
            var response = await _resumeParsingService.ParseResumeAsync(stream, file.FileName);
            
            return Ok(response);
        }
    }
}
