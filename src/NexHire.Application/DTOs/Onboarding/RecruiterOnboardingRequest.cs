namespace NexHire.Application.DTOs.Onboarding
{
    public class RecruiterOnboardingRequest
    {
        public string CompanyName { get; set; } = null!;
        public string Industry { get; set; } = null!;
        public string Size { get; set; } = null!;
        public string Designation { get; set; } = null!;
    }
}
