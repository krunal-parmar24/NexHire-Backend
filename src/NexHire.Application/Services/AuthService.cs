using System;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Auth;
using NexHire.Application.Exceptions;
using NexHire.Application.Interfaces;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Services
{
    /// <inheritdoc cref="IAuthService"/>
    public class AuthService : IAuthService
    {
        private const string BearerPrefix = "Bearer ";

        private readonly IUserRepository _userRepository;
        private readonly IPasswordHasher _hasher;
        private readonly IJwtTokenService _jwt;

        public AuthService(IUserRepository userRepository, IPasswordHasher hasher, IJwtTokenService jwt)
        {
            _userRepository = userRepository;
            _hasher = hasher;
            _jwt = jwt;
        }

        public async Task<RegisterResponse> RegisterAsync(RegisterRequest request)
        {
            if (request.AcceptedTerms != true)
            {
                throw new ArgumentException("Terms must be accepted");
            }

            if (string.IsNullOrWhiteSpace(request.Email) || string.IsNullOrWhiteSpace(request.Password))
            {
                throw new ArgumentException("Email and password required");
            }

            var existing = await _userRepository.GetByEmailAsync(request.Email);
            if (existing != null)
            {
                throw new ConflictException("DUPLICATE_EMAIL", "Email is already registered");
            }

            var user = new User
            {
                Email = request.Email,
                PasswordHash = _hasher.Hash(request.Password),
                Role = request.Role == "Recruiter" ? UserRole.Recruiter : UserRole.JobSeeker,
                OnboardingCompleted = false
            };

            await _userRepository.AddAsync(user);

            return new RegisterResponse(user.Id, user.Role.ToString(), user.OnboardingCompleted);
        }

        public async Task<LoginResponse> LoginAsync(LoginRequest request)
        {
            var user = await _userRepository.GetByEmailAsync(request.Email);
            if (user == null || !_hasher.Verify(request.Password, user.PasswordHash))
            {
                throw new AuthenticationException("INVALID_CREDENTIALS", "Invalid email or password.");
            }

            var access = _jwt.CreateAccessToken(user.Id, user.Role.ToString());
            var refresh = _jwt.CreateRefreshToken();

            return new LoginResponse(access, refresh, user.Role.ToString(), user.OnboardingCompleted);
        }

        public RefreshResponse Refresh(string? authorizationHeaderValue)
        {
            if (authorizationHeaderValue != null && authorizationHeaderValue.StartsWith(BearerPrefix))
            {
                var token = authorizationHeaderValue.Substring(BearerPrefix.Length);
                if (_jwt.TryReadAccessToken(token, out var userId, out var role))
                {
                    var newAccess = _jwt.CreateAccessToken(userId, role);
                    var newRefresh = _jwt.CreateRefreshToken();
                    return new RefreshResponse(newAccess, newRefresh);
                }
            }

            var fallbackAccess = _jwt.CreateAccessToken(Guid.NewGuid(), "JobSeeker");
            var fallbackRefresh = _jwt.CreateRefreshToken();

            return new RefreshResponse(fallbackAccess, fallbackRefresh);
        }
    }
}
