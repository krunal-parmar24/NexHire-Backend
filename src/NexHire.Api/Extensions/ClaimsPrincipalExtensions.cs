using System;
using System.Security.Claims;

namespace NexHire.Api.Extensions
{
    /// <summary>
    /// Helpers for extracting the authenticated user's id from claims, replacing the
    /// duplicated inline parsing logic previously repeated across controllers.
    /// </summary>
    public static class ClaimsPrincipalExtensions
    {
        /// <summary>
        /// Gets the current user's id from the <see cref="ClaimTypes.NameIdentifier"/> claim.
        /// Throws if the claim is missing or not a valid <see cref="Guid"/>.
        /// </summary>
        public static Guid GetUserId(this ClaimsPrincipal principal)
        {
            return Guid.Parse(principal.FindFirstValue(ClaimTypes.NameIdentifier)!);
        }

        /// <summary>
        /// Attempts to get the current user's id from the <see cref="ClaimTypes.NameIdentifier"/> claim.
        /// Returns <c>false</c> instead of throwing when the claim is missing or invalid.
        /// </summary>
        public static bool TryGetUserId(this ClaimsPrincipal principal, out Guid userId)
        {
            var value = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            return Guid.TryParse(value, out userId);
        }
    }
}
