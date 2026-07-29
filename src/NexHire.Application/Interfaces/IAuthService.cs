using NexHire.Application.DTOs.Auth;

namespace NexHire.Application.Interfaces
{
    /// <summary>Handles user registration, login, and access-token refresh.</summary>
    public interface IAuthService
    {
        /// <summary>Registers a new user account after validating terms acceptance and email uniqueness.</summary>
        Task<RegisterResponse> RegisterAsync(RegisterRequest request);

        /// <summary>Authenticates a user by email/password and issues access/refresh tokens.</summary>
        Task<LoginResponse> LoginAsync(LoginRequest request);

        /// <summary>
        /// Issues a new access/refresh token pair. When the supplied bearer token can be read,
        /// the new tokens carry over its user id and role claims; otherwise a fallback token is issued.
        /// </summary>
        RefreshResponse Refresh(string? authorizationHeaderValue);
    }
}
