using System;
using System.Linq;
using System.Security.Claims;
using System.Text.Json;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using NexHire.Api.Common.Constants;
using NexHire.Infrastructure.Persistence;

namespace NexHire.Api.Middleware
{
    /// <summary>
    /// Blocks authenticated requests to any endpoint other than auth/onboarding routes
    /// until the current user has completed role-specific onboarding.
    /// </summary>
    public class OnboardingGuardMiddleware
    {
        private readonly RequestDelegate _next;

        public OnboardingGuardMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, NexHireDbContext dbContext)
        {
            var path = context.Request.Path.Value?.ToLower() ?? "";

            // Bypass auth routes and onboarding routes
            if (path.StartsWith(ApiRouteConstants.AuthRoutePrefix) || path.StartsWith(ApiRouteConstants.OnboardingRoutePrefix))
            {
                await _next(context);
                return;
            }

            // If user is authenticated, check onboarding status
            if (context.User.Identity?.IsAuthenticated == true)
            {
                var userIdClaim = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (Guid.TryParse(userIdClaim, out var userId))
                {
                    var user = await dbContext.Users.AsNoTracking().FirstOrDefaultAsync(u => u.Id == userId);
                    if (user != null && !user.OnboardingCompleted)
                    {
                        context.Response.StatusCode = StatusCodes.Status403Forbidden;
                        context.Response.ContentType = "application/json";
                        var error = new { error = new { code = "ONBOARDING_REQUIRED", message = "You must complete onboarding to access this resource." } };
                        await context.Response.WriteAsync(JsonSerializer.Serialize(error));
                        return;
                    }
                }
            }

            await _next(context);
        }
    }
}
