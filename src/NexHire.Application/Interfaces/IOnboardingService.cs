using System;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Onboarding;

namespace NexHire.Application.Interfaces
{
    public interface IOnboardingService
    {
        Task<bool> CompleteJobSeekerOnboardingAsync(Guid userId, JobSeekerOnboardingRequest req);
        Task<bool> CompleteRecruiterOnboardingAsync(Guid userId, RecruiterOnboardingRequest req);
    }
}
