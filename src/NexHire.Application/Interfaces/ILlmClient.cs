using System.Threading.Tasks;
using NexHire.Application.DTOs.Onboarding;

namespace NexHire.Application.Interfaces
{
    public interface ILlmClient
    {
        Task<ParsedFieldsDto> ParseResumeTextAsync(string text);
        Task<int> GetSemanticTitleMatchAsync(string candidateTitle, string jobTitle);
    }
}
