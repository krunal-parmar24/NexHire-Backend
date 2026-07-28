using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using NexHire.Application.Interfaces;
using NexHire.Application.Services;
using NexHire.Domain.Entities;
using Xunit;

namespace NexHire.Application.Tests
{
    public class AtsScoringServiceTests
    {
        private readonly Mock<IJobRepository> _jobRepositoryMock = new();
        private readonly Mock<IUserRepository> _userRepositoryMock = new();
        private readonly Mock<ILlmClient> _llmClientMock = new();
        private readonly AtsScoringService _sut;

        public AtsScoringServiceTests()
        {
            _sut = new AtsScoringService(
                _jobRepositoryMock.Object,
                _userRepositoryMock.Object,
                _llmClientMock.Object
            );

            // Default: LLM returns 70 for domain/title match unless overridden per test.
            _llmClientMock
                .Setup(x =>
                    x.GetSemanticTitleMatchAsync(It.IsAny<string>(), It.IsAny<string>())
                )
                .ReturnsAsync(70);
        }

        // ── Weight distribution ───────────────────────────────────────────────

        [Fact]
        public async Task GetMatchScoreAsync_WhenJdHasCerts_AppliesStandardWeighting()
        {
            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "Requires 5 years of experience with C# and AWS certification.",
                profileBuilder: p =>
                {
                    p.TotalExperienceYears = 5;
                    p.Skills = ["C#"];
                    p.Certifications = ["AWS Certified Developer"];
                    p.CurrentTitle = "Backend Engineer";
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            result.CertificationWeightRedistributed.Should().BeFalse();
            result.Breakdown.SkillsCoverage.Weight.Should().Be(40);
            result.Breakdown.ExperienceFit.Weight.Should().Be(25);
            result.Breakdown.CertificationMatch.Weight.Should().Be(20);
            result.Breakdown.DomainTitleMatch.Weight.Should().Be(15);
        }

        [Fact]
        public async Task GetMatchScoreAsync_WhenJdHasNoCerts_RedistributesWeightCorrectly()
        {
            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "3+ years of experience with React and TypeScript.",
                profileBuilder: p =>
                {
                    p.TotalExperienceYears = 3;
                    p.Skills = ["React", "TypeScript"];
                    p.Certifications = ["Scrum Master"]; // candidate has cert, but JD doesn't require one
                    p.CurrentTitle = "Frontend Developer";
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            result.CertificationWeightRedistributed.Should().BeTrue();
            result.Breakdown.SkillsCoverage.Weight.Should().Be(55);
            result.Breakdown.ExperienceFit.Weight.Should().Be(30);
            result.Breakdown.CertificationMatch.Weight.Should().Be(0);
            result.Breakdown.DomainTitleMatch.Weight.Should().Be(15);
            result.Breakdown.CertificationMatch.Score.Should().Be(0);
        }

        // ── Certification match ───────────────────────────────────────────────

        [Fact]
        public async Task GetMatchScoreAsync_WhenJdRequiresCertAndUserHasIt_CertScoreIs100()
        {
            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "AWS certification required for this role.",
                profileBuilder: p =>
                {
                    p.Certifications = ["AWS Certified Solutions Architect"];
                    p.CurrentTitle = "Cloud Engineer";
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            result.CertificationWeightRedistributed.Should().BeFalse();
            result.Breakdown.CertificationMatch.Score.Should().Be(100);
        }

        [Fact]
        public async Task GetMatchScoreAsync_WhenJdRequiresCertButUserLacksIt_CertScoreIsZero()
        {
            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "AWS certification is required.",
                profileBuilder: p =>
                {
                    p.Certifications = []; // no certs
                    p.CurrentTitle = "DevOps Engineer";
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            result.CertificationWeightRedistributed.Should().BeFalse();
            result.Breakdown.CertificationMatch.Score.Should().Be(0);
        }

        [Fact]
        public async Task GetMatchScoreAsync_WhenUserCertDoesNotMatchJdCert_CertScoreIsZero()
        {
            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "PMP certification preferred.",
                profileBuilder: p =>
                {
                    p.Certifications = ["AWS Certified Developer"]; // different cert
                    p.CurrentTitle = "Project Manager";
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            result.Breakdown.CertificationMatch.Score.Should().Be(0);
        }

        // ── Experience fit ────────────────────────────────────────────────────

        [Fact]
        public async Task GetMatchScoreAsync_WhenCandidateMeetsExperience_ExpScoreIs100()
        {
            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "Minimum 3 years of experience with Node.js.",
                profileBuilder: p =>
                {
                    p.TotalExperienceYears = 5;
                    p.Skills = ["Node.js"];
                    p.CurrentTitle = "Backend Developer";
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            result.Breakdown.ExperienceFit.Score.Should().Be(100);
        }

        [Fact]
        public async Task GetMatchScoreAsync_WhenCandidateBelowExperience_ExpScoreIsProrated()
        {
            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "At least 6 years of experience required.",
                profileBuilder: p =>
                {
                    p.TotalExperienceYears = 3;
                    p.CurrentTitle = "Senior Developer";
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            // 3/6 = 50 %
            result.Breakdown.ExperienceFit.Score.Should().Be(50);
        }

        [Fact]
        public async Task GetMatchScoreAsync_WhenJdHasNoExperienceRequirement_ExpScoreIs100()
        {
            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "Looking for a creative designer with strong Figma skills.",
                profileBuilder: p =>
                {
                    p.TotalExperienceYears = 1;
                    p.Skills = ["Figma"];
                    p.CurrentTitle = "UI Designer";
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            result.Breakdown.ExperienceFit.Score.Should().Be(100);
        }

        // ── Domain/Title match ────────────────────────────────────────────────

        [Fact]
        public async Task GetMatchScoreAsync_WhenCandidateHasNoTitle_TitleScoreIsZero()
        {
            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "5 years of experience.",
                profileBuilder: p =>
                {
                    p.TotalExperienceYears = 5;
                    p.CurrentTitle = null; // no title → no alignment signal
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            result.Breakdown.DomainTitleMatch.Score.Should().Be(0);
            _llmClientMock.Verify(
                x => x.GetSemanticTitleMatchAsync(It.IsAny<string>(), It.IsAny<string>()),
                Times.Never
            );
        }

        [Fact]
        public async Task GetMatchScoreAsync_WhenCandidateHasTitle_InvokesLlmForTitleMatch()
        {
            _llmClientMock
                .Setup(x =>
                    x.GetSemanticTitleMatchAsync("Senior .NET Developer", "Backend Engineer")
                )
                .ReturnsAsync(90);

            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "Strong C# skills required.",
                jobTitle: "Backend Engineer",
                profileBuilder: p =>
                {
                    p.Skills = ["C#"];
                    p.CurrentTitle = "Senior .NET Developer";
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            result.Breakdown.DomainTitleMatch.Score.Should().Be(90);
        }

        // ── Overall score integrity ───────────────────────────────────────────

        [Fact]
        public async Task GetMatchScoreAsync_OverallScore_IsWithinValidRange()
        {
            var (jobId, userId) = ArrangeJobAndUser(
                requirements: "React, TypeScript, 4 years experience, AWS certification.",
                profileBuilder: p =>
                {
                    p.TotalExperienceYears = 4;
                    p.Skills = ["React", "TypeScript"];
                    p.Certifications = ["AWS Certified Developer"];
                    p.CurrentTitle = "Frontend Developer";
                }
            );

            var result = await _sut.GetMatchScoreAsync(jobId, userId, CancellationToken.None);

            result.OverallScore.Should().BeInRange(0, 100);
        }

        // ─────────────────────────────────────────────────────────────────────
        // Helpers
        // ─────────────────────────────────────────────────────────────────────

        private (Guid jobId, Guid userId) ArrangeJobAndUser(
            string requirements,
            Action<UserProfile> profileBuilder,
            string jobTitle = "Software Engineer",
            string description = ""
        )
        {
            var jobId = Guid.NewGuid();
            var userId = Guid.NewGuid();

            var job = new Job
            {
                Id = jobId,
                Title = jobTitle,
                Requirements = requirements,
                Description = description,
            };

            var profile = new UserProfile();
            profileBuilder(profile);

            var user = new User { Id = userId, Profile = profile };

            _jobRepositoryMock.Setup(r => r.GetByIdAsync(jobId)).ReturnsAsync(job);
            _userRepositoryMock.Setup(r => r.GetByIdAsync(userId)).ReturnsAsync(user);

            return (jobId, userId);
        }
    }
}
