using System;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Onboarding;
using NexHire.Application.Interfaces;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;

namespace NexHire.Application.Services
{
    /// <inheritdoc cref="IOnboardingService"/>
    public class OnboardingService : IOnboardingService
    {
        private readonly IUserRepository _userRepository;
        private readonly ICompanyRepository _companyRepository;

        public OnboardingService(IUserRepository userRepository, ICompanyRepository companyRepository)
        {
            _userRepository = userRepository;
            _companyRepository = companyRepository;
        }

        public async Task<bool> CompleteJobSeekerOnboardingAsync(Guid userId, JobSeekerOnboardingRequest req)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Role != UserRole.JobSeeker) return false;

            user.Profile = new UserProfile
            {
                FullName = req.FullName,
                Phone = req.Phone,
                CurrentTitle = req.CurrentTitle,
                TotalExperienceYears = req.TotalExperienceYears,
                Skills = req.Skills ?? new(),
                PreferredJobType = req.PreferredJobType,
                PreferredLocation = req.PreferredLocation,
                Certifications = req.Certifications ?? new(),
                PortfolioLinks = req.PortfolioLinks ?? new(),
                ExpectedSalaryRange = req.ExpectedSalaryRange
            };
            user.OnboardingCompleted = true;

            await _userRepository.UpdateAsync(user);
            return true;
        }

        public async Task<bool> CompleteRecruiterOnboardingAsync(Guid userId, RecruiterOnboardingRequest req)
        {
            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null || user.Role != UserRole.Recruiter) return false;

            var company = new Company
            {
                Name = req.CompanyName,
                Industry = req.Industry,
                Size = req.Size,
                RecruiterId = userId,
                VerificationStatus = VerificationStatus.Unverified
            };

            await _companyRepository.AddAsync(company);

            user.OnboardingCompleted = true;
            await _userRepository.UpdateAsync(user);
            return true;
        }
    }
}
