using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Api.Extensions;
using NexHire.Application.Interfaces;

namespace NexHire.Api.Controllers
{
    /// <summary>Recruiter-facing dashboard metrics endpoint.</summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Recruiter")]
    public class DashboardController : ControllerBase
    {
        private readonly IDashboardService _dashboardService;

        public DashboardController(IDashboardService dashboardService)
        {
            _dashboardService = dashboardService;
        }

        [HttpGet("recruiter")]
        public async Task<IActionResult> GetRecruiterDashboard()
        {
            if (!User.TryGetUserId(out var recruiterId))
                return Unauthorized();

            var result = await _dashboardService.GetRecruiterDashboardAsync(recruiterId);
            return Ok(result);
        }
    }
}
