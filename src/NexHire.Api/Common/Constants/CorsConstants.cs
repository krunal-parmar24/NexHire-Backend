namespace NexHire.Api.Common.Constants
{
    /// <summary>
    /// CORS policy names and allowed origins used when configuring the API host.
    /// </summary>
    internal static class CorsConstants
    {
        /// <summary>Name of the CORS policy that allows the local frontend dev server origins.</summary>
        public const string AllowFrontendPolicy = "AllowFrontend";

        /// <summary>Origins permitted to call the API during local development.</summary>
        public static readonly string[] AllowedFrontendOrigins =
        [
            "http://localhost:5173",
            "http://localhost:5174",
            "http://127.0.0.1:5173"
        ];
    }
}
