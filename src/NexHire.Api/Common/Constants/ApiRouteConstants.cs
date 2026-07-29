namespace NexHire.Api.Common.Constants
{
    /// <summary>
    /// Route-prefix literals shared by API middleware (e.g. onboarding guard bypass rules).
    /// </summary>
    internal static class ApiRouteConstants
    {
        /// <summary>Prefix for authentication endpoints, exempt from the onboarding guard.</summary>
        public const string AuthRoutePrefix = "/api/auth";

        /// <summary>Prefix for onboarding endpoints, exempt from the onboarding guard.</summary>
        public const string OnboardingRoutePrefix = "/api/onboarding";
    }
}
