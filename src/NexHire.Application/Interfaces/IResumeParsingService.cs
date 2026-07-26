using System.IO;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Onboarding;

namespace NexHire.Application.Interfaces
{
    public interface IResumeParsingService
    {
        Task<ParseResumeResponse> ParseResumeAsync(Stream fileStream, string fileName);
    }
}
