using System;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using NexHire.Application.Interfaces;
using System.Security.Claims;

namespace NexHire.Api.Controllers
{
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
            var userIdStr = User.FindFirstValue(ClaimTypes.NameIdentifier);
            if (string.IsNullOrEmpty(userIdStr) || !Guid.TryParse(userIdStr, out var recruiterId))
                return Unauthorized();

            var result = await _dashboardService.GetRecruiterDashboardAsync(recruiterId);
            return Ok(result);
        }
    }
}
