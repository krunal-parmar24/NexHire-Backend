using BCrypt.Net;
using NexHire.Application.Interfaces;

namespace NexHire.Infrastructure.Services
{
    /// <inheritdoc cref="IPasswordHasher"/>
    public class BcryptPasswordHasher : IPasswordHasher
    {
        public string Hash(string password)
        {
            return BCrypt.Net.BCrypt.EnhancedHashPassword(password);
        }

        public bool Verify(string password, string hash)
        {
            return BCrypt.Net.BCrypt.EnhancedVerify(password, hash);
        }
    }
}
