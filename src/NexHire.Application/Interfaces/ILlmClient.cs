using System.Threading.Tasks;
using NexHire.Application.DTOs.Onboarding;

namespace NexHire.Application.Interfaces
{
    /// <summary>Abstraction over the LLM provider used for resume parsing and ATS semantic title matching.</summary>
    public interface ILlmClient
    {
        /// <summary>Extracts structured resume fields from raw resume text.</summary>
        Task<ParsedFieldsDto> ParseResumeTextAsync(string text);

        /// <summary>Scores the semantic alignment (0-100) between a candidate's title and a job's title.</summary>
        Task<int> GetSemanticTitleMatchAsync(string candidateTitle, string jobTitle);
    }
}
