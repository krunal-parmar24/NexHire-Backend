using System.IO;

namespace NexHire.Application.Interfaces
{
    public interface ITextExtractor
    {
        string ExtractText(Stream fileStream, string fileName, int maxCharacters = 12000);
    }
}
