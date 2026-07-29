using System;

namespace NexHire.Application.Exceptions
{
    /// <summary>Thrown when an operation conflicts with the current state of a resource (e.g. duplicate application, invalid withdrawal).</summary>
    public class ConflictException : Exception
    {
        /// <summary>Machine-readable error code surfaced to API clients.</summary>
        public string Code { get; }

        public ConflictException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
