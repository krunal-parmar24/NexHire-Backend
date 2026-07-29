using System;
using System.IO;
using System.Text;
using DocumentFormat.OpenXml.Packaging;
using Microsoft.Extensions.Logging;
using NexHire.Application.Interfaces;
using UglyToad.PdfPig;

namespace NexHire.Infrastructure.DocumentExtraction
{
    /// <inheritdoc cref="ITextExtractor"/>
    public class TextExtractor : ITextExtractor
    {
        private readonly ILogger<TextExtractor> _logger;

        public TextExtractor(ILogger<TextExtractor> logger)
        {
            _logger = logger;
        }

        public string ExtractText(Stream fileStream, string fileName, int maxCharacters = 12000)
        {
            if (string.IsNullOrWhiteSpace(fileName)) return string.Empty;

            var ext = Path.GetExtension(fileName).ToLowerInvariant();
            string text = string.Empty;

            try
            {
                if (ext == ".pdf")
                {
                    text = ExtractFromPdf(fileStream);
                }
                else if (ext == ".docx")
                {
                    text = ExtractFromDocx(fileStream);
                }
            }
            catch (Exception ex)
            {
                // Fallback: text extraction failures are non-fatal for the caller, so we log and return empty.
                _logger.LogWarning(ex, "Failed to extract text from file {FileName}", fileName);
                return string.Empty;
            }

            if (text.Length > maxCharacters)
            {
                return text.Substring(0, maxCharacters);
            }

            return text;
        }

        private string ExtractFromPdf(Stream stream)
        {
            using var document = PdfDocument.Open(stream);
            var sb = new StringBuilder();
            foreach (var page in document.GetPages())
            {
                sb.AppendLine(page.Text);
            }
            return sb.ToString();
        }

        private string ExtractFromDocx(Stream stream)
        {
            using var wordDocument = WordprocessingDocument.Open(stream, false);
            var body = wordDocument.MainDocumentPart?.Document?.Body;
            return body?.InnerText ?? string.Empty;
        }
    }
}
