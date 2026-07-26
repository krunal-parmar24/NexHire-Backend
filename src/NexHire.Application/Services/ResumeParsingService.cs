using System.IO;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Onboarding;
using NexHire.Application.Interfaces;

namespace NexHire.Application.Services
{
    public class ResumeParsingService : IResumeParsingService
    {
        private readonly ITextExtractor _textExtractor;
        private readonly ILlmClient _llmClient;

        public ResumeParsingService(ITextExtractor textExtractor, ILlmClient llmClient)
        {
            _textExtractor = textExtractor;
            _llmClient = llmClient;
        }

        public async Task<ParseResumeResponse> ParseResumeAsync(Stream fileStream, string fileName)
        {
            var text = _textExtractor.ExtractText(fileStream, fileName);
            var parsedFields = await _llmClient.ParseResumeTextAsync(text);
            
            return new ParseResumeResponse
            {
                ParsedFields = parsedFields,
                CreditsDeducted = 0
            };
        }
    }
}
