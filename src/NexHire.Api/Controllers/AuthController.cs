using Microsoft.AspNetCore.Mvc;
using NexHire.Application.DTOs.Auth;
using NexHire.Application.Interfaces;

namespace NexHire.Api.Controllers
{
    /// <summary>Registration, login, and token-refresh endpoints.</summary>
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IAuthService _authService;

        public AuthController(IAuthService authService)
        {
            _authService = authService;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            var response = await _authService.RegisterAsync(req);
            return Created("/api/auth/register", response);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var response = await _authService.LoginAsync(req);
            return Ok(response);
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest req)
        {
            if (string.IsNullOrWhiteSpace(req.RefreshToken))
            {
                return BadRequest(new { error = "MISSING_REFRESH_TOKEN" });
            }

            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            var response = _authService.Refresh(authHeader);
            return Ok(response);
        }
    }
}
