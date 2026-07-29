namespace NexHire.Application.Common.Constants
{
    /// <summary>
    /// Centralized file-upload limits used across onboarding/document-related endpoints.
    /// </summary>
    public static class FileUploadConstants
    {
        /// <summary>Maximum accepted size, in bytes, for an uploaded resume file (1 MB).</summary>
        public const int MaxResumeSizeBytes = 1 * 1024 * 1024;
    }
}
