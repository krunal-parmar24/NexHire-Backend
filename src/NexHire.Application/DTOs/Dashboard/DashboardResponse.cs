namespace NexHire.Application.DTOs.Dashboard
{
    public class DashboardResponse
    {
        public int ActiveJobPostings { get; set; }
        public int TotalApplicants { get; set; }
        public int PendingReview { get; set; }
        public string VerificationStatus { get; set; } = string.Empty;
    }
}
