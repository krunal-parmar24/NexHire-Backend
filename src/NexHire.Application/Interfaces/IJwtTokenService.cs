using System;

namespace NexHire.Application.Interfaces
{
    /// <summary>Issues signed JWT access tokens and opaque refresh tokens for authenticated users.</summary>
    public interface IJwtTokenService
    {
        /// <summary>Creates a short-lived signed JWT access token containing the user id and role claims.</summary>
        string CreateAccessToken(Guid userId, string role);

        /// <summary>Creates a cryptographically random opaque refresh token.</summary>
        string CreateRefreshToken();

        /// <summary>
        /// Attempts to read the user id and role claims from a raw (unvalidated) access token.
        /// Returns <c>false</c> when the token cannot be parsed or has no valid user id claim.
        /// </summary>
        bool TryReadAccessToken(string token, out Guid userId, out string role);
    }
}
