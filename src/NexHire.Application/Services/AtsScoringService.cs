using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using NexHire.Application.DTOs.Jobs;
using NexHire.Application.Exceptions;
using NexHire.Application.Interfaces;

namespace NexHire.Application.Services
{
    /// <inheritdoc cref="IAtsScoringService"/>
    public class AtsScoringService : IAtsScoringService
    {
        // Certification keywords used ONLY to detect whether the JD mentions
        // certification requirements. Matching against the candidate's profile
        // is done by checking whether the candidate's cert strings contain any
        // of the exact cert tokens found in the JD text.
        private static readonly string[] CertDetectionKeywords =
        [
            "certification",
            "certified",
            "certificate",
            "aws",
            "azure",
            "gcp",
            "pmp",
            "cism",
            "cissp",
            "ckad",
            "cka",
            "scrum",
            "comptia",
            "ccna",
            "ccnp",
        ];

        // Common English stop-words excluded from JD skill token extraction.
        private static readonly HashSet<string> StopWords = new(StringComparer.OrdinalIgnoreCase)
        {
            "the", "a", "an", "and", "or", "in", "of", "to", "for", "with",
            "on", "at", "by", "is", "are", "be", "as", "we", "our", "your",
            "you", "have", "has", "will", "must", "should", "can", "may",
            "years", "year", "experience", "preferred", "required", "strong",
            "excellent", "ability", "skills", "skill", "knowledge", "good",
            "plus", "bonus", "including", "etc", "other", "ability", "work",
            "team", "using", "well", "design", "build", "develop",
        };

        private readonly IJobRepository _jobRepository;
        private readonly IUserRepository _userRepository;
        private readonly ILlmClient _llmClient;

        public AtsScoringService(
            IJobRepository jobRepository,
            IUserRepository userRepository,
            ILlmClient llmClient
        )
        {
            _jobRepository = jobRepository;
            _userRepository = userRepository;
            _llmClient = llmClient;
        }

        public async Task<MatchScoreResponse> GetMatchScoreAsync(
            Guid jobId,
            Guid userId,
            CancellationToken ct
        )
        {
            var job = await _jobRepository.GetByIdAsync(jobId);
            if (job == null)
                throw new NotFoundException("JOB_NOT_FOUND", $"Job {jobId} not found.");

            var user = await _userRepository.GetByIdAsync(userId);
            if (user == null)
                throw new NotFoundException("JOB_NOT_FOUND", $"User {userId} not found.");

            var profile = user.Profile ?? new Domain.Entities.UserProfile();

            // Combine requirements + description for full JD text scanning.
            var jdFull = $"{job.Requirements} {job.Description}".ToLowerInvariant();

            // ── Pillar 1: Skills Coverage ─────────────────────────────────
            // Score = (JD skills matched by candidate) / (total JD skills) × 100
            // If JD has no extractable skills, fall back to a token-overlap approach.
            var jdSkills = ExtractJdSkillTokens(jdFull, profile.Skills);
            int skillsScore = ComputeSkillsCoverage(jdSkills, profile.Skills);

            // ── Pillar 2: Experience Fit ──────────────────────────────────
            // Score = candidate years / minimum JD years × 100 (capped at 100).
            // When no explicit minimum is found the candidate is not penalised.
            int expScore = ComputeExperienceFit(jdFull, profile.TotalExperienceYears ?? 0);

            // ── Pillar 3: Certification Match ─────────────────────────────
            // Detect whether the JD requires any certification by scanning for
            // cert-detection keywords. If yes, check the candidate's
            // Certifications list for an exact/equivalent cert token match.
            bool jdHasCerts = CertDetectionKeywords.Any(k => jdFull.Contains(k));
            int certScore = jdHasCerts
                ? ComputeCertificationMatch(jdFull, profile.Certifications)
                : 0;

            // ── Dynamic Weight Redistribution ─────────────────────────────
            // When the JD has no certification requirements, the 20 % cert
            // weight is redistributed: +15 % to Skills, +5 % to Experience.
            // Domain/Title always stays at 15 %.
            int weightSkills = jdHasCerts ? 40 : 55;
            int weightExp = jdHasCerts ? 25 : 30;
            int weightCert = jdHasCerts ? 20 : 0;
            const int weightTitle = 15;

            // ── Pillar 4: Domain/Title Match ──────────────────────────────
            // Semantic alignment via LLM call. If the candidate has no title
            // on their profile there is no alignment signal → score 0.
            int titleScore = 0;
            if (!string.IsNullOrWhiteSpace(profile.CurrentTitle))
            {
                titleScore = await _llmClient.GetSemanticTitleMatchAsync(
                    profile.CurrentTitle,
                    job.Title
                );
            }

            // ── Overall Score ─────────────────────────────────────────────
            double totalScore =
                (skillsScore * weightSkills / 100.0)
                + (expScore * weightExp / 100.0)
                + (certScore * weightCert / 100.0)
                + (titleScore * weightTitle / 100.0);

            return new MatchScoreResponse
            {
                JobId = jobId,
                OverallScore = (int)Math.Round(totalScore),
                CertificationWeightRedistributed = !jdHasCerts,
                Breakdown = new MatchScoreBreakdown
                {
                    SkillsCoverage = new PillarScore
                    {
                        Weight = weightSkills,
                        Score = skillsScore,
                    },
                    ExperienceFit = new PillarScore { Weight = weightExp, Score = expScore },
                    CertificationMatch = new PillarScore
                    {
                        Weight = weightCert,
                        Score = certScore,
                    },
                    DomainTitleMatch = new PillarScore
                    {
                        Weight = weightTitle,
                        Score = titleScore,
                    },
                },
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // Private helpers
        // ─────────────────────────────────────────────────────────────────────

        /// <summary>
        /// Extracts a deduplicated set of "skill-like" tokens from the JD text.
        /// Strategy: take every word/phrase from the candidate's Skills list
        /// that also appears in the JD (known-skill intersection), then
        /// supplement with short (2–20 char) capitalised tokens from the JD
        /// that look like technology names (e.g., React, .NET, SQL, TypeScript).
        /// The union gives the best approximation of "skills the JD cares about"
        /// without requiring a fixed lookup dictionary.
        /// </summary>
        private static HashSet<string> ExtractJdSkillTokens(
            string jdLower,
            IList<string> candidateSkills
        )
        {
            var tokens = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // Candidate skills that appear in the JD are definitely JD skills.
            foreach (var skill in candidateSkills)
            {
                if (!string.IsNullOrWhiteSpace(skill) && jdLower.Contains(skill.ToLowerInvariant()))
                    tokens.Add(skill.Trim());
            }

            // Also capture short tech-looking tokens from the raw JD text
            // (words that are not pure stop-words and are plausible tech names).
            var wordPattern = new Regex(@"[\w#\.\+\-]+");
            foreach (Match m in wordPattern.Matches(jdLower))
            {
                var word = m.Value.Trim();
                if (
                    word.Length >= 2
                    && word.Length <= 25
                    && !StopWords.Contains(word)
                    && !int.TryParse(word, out _)
                )
                    tokens.Add(word);
            }

            return tokens;
        }

        /// <summary>
        /// Skills Coverage = (JD skills covered by candidate) / (JD skills total) × 100.
        /// Falls back to 0 when neither the JD nor the candidate has any skills.
        /// </summary>
        private static int ComputeSkillsCoverage(
            HashSet<string> jdSkills,
            IList<string> candidateSkills
        )
        {
            if (jdSkills.Count == 0 || candidateSkills.Count == 0)
                return 0;

            var candidateLower = candidateSkills
                .Select(s => s.ToLowerInvariant())
                .ToHashSet();

            int matched = jdSkills.Count(jds => candidateLower.Contains(jds.ToLowerInvariant()));
            int score = (int)Math.Round((double)matched / jdSkills.Count * 100);
            return Math.Clamp(score, 0, 100);
        }

        /// <summary>
        /// Experience Fit:
        ///  - No explicit minimum found in JD → score 100 (no penalty).
        ///  - Candidate meets or exceeds the minimum → score 100.
        ///  - Candidate is under → prorated score (candidate / minimum) × 100.
        /// </summary>
        private static int ComputeExperienceFit(string jdLower, int candidateYears)
        {
            // Match patterns like "5+ years", "3-5 years", "minimum 4 years", etc.
            var expMatch = Regex.Match(
                jdLower,
                @"(?:minimum\s+|at\s+least\s+)?(\d+)\s*(?:\+|\s*[-–]\s*\d+)?\s*(?:years?|yrs?)(?:\s*of\s*experience)?",
                RegexOptions.IgnoreCase
            );

            if (!expMatch.Success || !int.TryParse(expMatch.Groups[1].Value, out int minYears))
                return 100; // No explicit requirement → full score

            if (candidateYears >= minYears)
                return 100;

            if (candidateYears <= 0)
                return 0;

            return Math.Clamp((int)Math.Round((double)candidateYears / minYears * 100), 0, 99);
        }

        /// <summary>
        /// Certification Match (only called when the JD signals cert requirements).
        /// A positive match requires the candidate's cert entry to contain the
        /// same cert token that appears in the JD (e.g., both contain "aws").
        /// Result is binary: 100 if any cert matches, 0 otherwise.
        /// </summary>
        private static int ComputeCertificationMatch(
            string jdLower,
            IList<string> candidateCertifications
        )
        {
            if (candidateCertifications.Count == 0)
                return 0;

            // Extract which cert tokens are actually present in this JD.
            var jdCertTokens = CertDetectionKeywords
                .Where(k => jdLower.Contains(k))
                .ToList();

            foreach (var userCert in candidateCertifications)
            {
                var userCertLower = userCert.ToLowerInvariant();
                if (jdCertTokens.Any(token => userCertLower.Contains(token)))
                    return 100;
            }

            return 0;
        }
    }
}
