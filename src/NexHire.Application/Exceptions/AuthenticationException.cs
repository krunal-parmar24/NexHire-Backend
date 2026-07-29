namespace NexHire.Application.Exceptions
{
    /// <summary>Thrown when authentication fails (e.g. invalid credentials during login).</summary>
    public class AuthenticationException : Exception
    {
        /// <summary>Machine-readable error code surfaced to API clients.</summary>
        public string Code { get; }

        public AuthenticationException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
