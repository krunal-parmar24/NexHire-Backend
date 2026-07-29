namespace NexHire.Application.DTOs.Auth
{
    /// <summary>Registration request payload.</summary>
    public record RegisterRequest(string Email, string Password, string Role, bool AcceptedTerms);

    /// <summary>Login request payload.</summary>
    public record LoginRequest(string Email, string Password);

    /// <summary>Refresh-token request payload.</summary>
    public record RefreshRequest(string RefreshToken);

    /// <summary>Response returned after successful registration.</summary>
    public record RegisterResponse(System.Guid UserId, string Role, bool OnboardingCompleted);

    /// <summary>Response returned after successful login, containing the issued tokens.</summary>
    public record LoginResponse(string AccessToken, string RefreshToken, string Role, bool OnboardingCompleted);

    /// <summary>Response returned after a token refresh, containing the newly issued tokens.</summary>
    public record RefreshResponse(string AccessToken, string RefreshToken);
}
