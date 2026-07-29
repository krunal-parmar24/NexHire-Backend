using System.IO;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Onboarding;

namespace NexHire.Application.Interfaces
{
    /// <summary>Extracts text from an uploaded resume file and parses it into structured fields via the LLM.</summary>
    public interface IResumeParsingService
    {
        /// <summary>Extracts text from the given resume file stream and parses it into structured fields.</summary>
        Task<ParseResumeResponse> ParseResumeAsync(Stream fileStream, string fileName);
    }
}
