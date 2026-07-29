using System;

namespace NexHire.Application.Exceptions
{
    /// <summary>Thrown when a requested resource (job, application, user, etc.) does not exist.</summary>
    public class NotFoundException : Exception
    {
        /// <summary>Machine-readable error code surfaced to API clients. Defaults to "NOT_FOUND".</summary>
        public string Code { get; }

        public NotFoundException(string message) : base(message)
        {
            Code = "NOT_FOUND";
        }

        public NotFoundException(string code, string message) : base(message)
        {
            Code = code;
        }
    }
}
