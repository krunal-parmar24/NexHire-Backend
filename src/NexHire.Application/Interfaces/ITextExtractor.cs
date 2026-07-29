using System.IO;

namespace NexHire.Application.Interfaces
{
    /// <summary>Extracts plain text from supported document formats (PDF, DOCX).</summary>
    public interface ITextExtractor
    {
        /// <summary>Extracts text from the given file stream, truncated to <paramref name="maxCharacters"/>. Returns an empty string on unsupported formats or extraction errors.</summary>
        string ExtractText(Stream fileStream, string fileName, int maxCharacters = 12000);
    }
}
