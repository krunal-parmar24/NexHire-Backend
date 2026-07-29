namespace NexHire.Application.Interfaces
{
    /// <summary>Hashes and verifies user passwords.</summary>
    public interface IPasswordHasher
    {
        /// <summary>Produces a salted hash for the given plaintext password.</summary>
        string Hash(string password);

        /// <summary>Verifies a plaintext password against a previously produced hash.</summary>
        bool Verify(string password, string hash);
    }
}
