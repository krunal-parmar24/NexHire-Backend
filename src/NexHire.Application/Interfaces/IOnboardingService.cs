using System;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Onboarding;

namespace NexHire.Application.Interfaces
{
    /// <summary>Handles role-specific onboarding completion for job seekers and recruiters.</summary>
    public interface IOnboardingService
    {
        /// <summary>Completes job-seeker onboarding by attaching the submitted profile; returns false if the user is not a job seeker or not found.</summary>
        Task<bool> CompleteJobSeekerOnboardingAsync(Guid userId, JobSeekerOnboardingRequest req);

        /// <summary>Completes recruiter onboarding by creating the recruiter's company; returns false if the user is not a recruiter or not found.</summary>
        Task<bool> CompleteRecruiterOnboardingAsync(Guid userId, RecruiterOnboardingRequest req);
    }
}
