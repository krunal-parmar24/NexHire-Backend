using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using NexHire.Infrastructure.Persistence;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;
using NexHire.Infrastructure.Services;

namespace NexHire.Api.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly NexHireDbContext _db;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenService _jwt;

        public AuthController(NexHireDbContext db, IPasswordHasher hasher, IJwtTokenService jwt)
        {
            _db = db;
            _hasher = hasher;
            _jwt = jwt;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest req)
        {
            if (req.AcceptedTerms != true)
            {
                return BadRequest(new { error = "Terms must be accepted" });
            }

            if (string.IsNullOrWhiteSpace(req.Email) || string.IsNullOrWhiteSpace(req.Password))
            {
                return BadRequest(new { error = "Email and password required" });
            }

            var existing = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (existing != null)
            {
                return Conflict(new { error = "DUPLICATE_EMAIL" });
            }

            var user = new User
            {
                Email = req.Email,
                PasswordHash = _hasher.Hash(req.Password),
                Role = req.Role == "Recruiter" ? UserRole.Recruiter : UserRole.JobSeeker,
                OnboardingCompleted = false
            };

            _db.Users.Add(user);
            await _db.SaveChangesAsync();

            return Created("/api/auth/register", new { userId = user.Id, role = user.Role.ToString(), onboardingCompleted = user.OnboardingCompleted });
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginRequest req)
        {
            var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == req.Email);
            if (user == null)
            {
                return Unauthorized(new { error = "INVALID_CREDENTIALS" });
            }

            if (!_hasher.Verify(req.Password, user.PasswordHash))
            {
                return Unauthorized(new { error = "INVALID_CREDENTIALS" });
            }

            var access = _jwt.CreateAccessToken(user.Id, user.Role.ToString());
            var refresh = _jwt.CreateRefreshToken();

            return Ok(new { accessToken = access, refreshToken = refresh, role = user.Role.ToString(), onboardingCompleted = user.OnboardingCompleted });
        }

        [HttpPost("refresh")]
        public IActionResult Refresh([FromBody] RefreshRequest req)
        {
            // NOTE: For Day 1 this is a stateless refresh placeholder.
            // Full refresh-token rotation & storage will be implemented in later days per the spec.
            if (string.IsNullOrWhiteSpace(req.RefreshToken))
            {
                return BadRequest(new { error = "MISSING_REFRESH_TOKEN" });
            }

            // In a proper implementation, validate & rotate refresh tokens stored server-side.
            var newAccess = _jwt.CreateAccessToken(Guid.NewGuid(), "JobSeeker");
            var newRefresh = _jwt.CreateRefreshToken();

            return Ok(new { accessToken = newAccess, refreshToken = newRefresh });
        }

        public record RegisterRequest(string Email, string Password, string Role, bool AcceptedTerms);
        public record LoginRequest(string Email, string Password);
        public record RefreshRequest(string RefreshToken);
    }
}
