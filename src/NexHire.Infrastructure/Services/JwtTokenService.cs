using System;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.IdentityModel.Tokens;
using System.Security.Cryptography;

namespace NexHire.Infrastructure.Services
{
    public interface IJwtTokenService
    {
        string CreateAccessToken(Guid userId, string role);
        string CreateRefreshToken();
    }

    public class JwtTokenService : IJwtTokenService
    {
        private readonly string _signingKey;
        private readonly int _accessTokenMinutes;

        public JwtTokenService(IConfiguration config)
        {
            _signingKey = config["Jwt:Key"] ?? throw new ArgumentNullException("Jwt:SigningKey");
            _accessTokenMinutes = int.TryParse(config["Jwt:AccessTokenExpiryMinutes"], out var m) ? m : 15;
        }

        public string CreateAccessToken(Guid userId, string role)
        {
            var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_signingKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim(JwtRegisteredClaimNames.Sub, userId.ToString()),
                new Claim(ClaimTypes.Role, role)
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.UtcNow.AddMinutes(_accessTokenMinutes),
                signingCredentials: creds);

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public string CreateRefreshToken()
        {
            return Convert.ToBase64String(RandomNumberGenerator.GetBytes(64));
        }
    }
}
