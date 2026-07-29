using NexHire.Infrastructure.Persistence;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;
using NexHire.Application.Interfaces;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace NexHire.Infrastructure.Persistence.Seed
{
    public static class DevSeeder
    {
        public static async Task SeedAsync(NexHireDbContext db, IPasswordHasher hasher)
        {
            await db.Database.MigrateAsync();

            if (!await db.Users.AnyAsync(u => u.Email == "demo.seeker@example.com"))
            {
                var seeker = new User
                {
                    Email = "demo.seeker@example.com",
                    PasswordHash = hasher.Hash("Password123!"),
                    Role = UserRole.JobSeeker,
                    OnboardingCompleted = false
                };
                db.Users.Add(seeker);
            }

            if (!await db.Users.AnyAsync(u => u.Email == "demo.recruiter@example.com"))
            {
                var recruiter = new User
                {
                    Email = "demo.recruiter@example.com",
                    PasswordHash = hasher.Hash("Password123!"),
                    Role = UserRole.Recruiter,
                    OnboardingCompleted = false
                };
                db.Users.Add(recruiter);
                await db.SaveChangesAsync(); // Save user to generate Id

                // Seed demo company
                if (!await db.Companies.AnyAsync())
                {
                    var company = new Company
                    {
                        Name = "Acme Corp",
                        Industry = "Technology",
                        Size = "100-500",
                        RecruiterId = recruiter.Id,
                        VerificationStatus = VerificationStatus.Verified
                    };
                    db.Companies.Add(company);
                    await db.SaveChangesAsync();
                }
            }

            var seededCompany = await db.Companies.FirstOrDefaultAsync(c => c.Name == "Acme Corp");
            if (seededCompany != null && !await db.Jobs.AnyAsync())
            {
                var jobs = new List<Job>
                {
                    new Job
                    {
                        CompanyId = seededCompany.Id,
                        Title = "Backend Engineer",
                        Description = "We are looking for a Senior Backend Engineer to join our core API team. You will design, build, and maintain high-performance microservices, optimize database queries, and integrate with external APIs.",
                        Requirements = "Strong experience with C#, .NET 8/9, EF Core, and PostgreSQL. Familiarity with Redis caching and Docker is highly preferred.",
                        Location = "Remote",
                        JobType = "Full-time",
                        SalaryRange = "12-18 LPA",
                        RemoteType = "Remote",
                        Status = JobStatus.Active,
                        ScreeningQuestions = new List<ScreeningQuestion>
                        {
                            new ScreeningQuestion { QuestionId = "q1_dotnet_exp", Label = "Years of experience with ASP.NET Core?", Type = "numeric", Required = true },
                            new ScreeningQuestion { QuestionId = "q2_postgres", Label = "Do you have experience with PostgreSQL?", Type = "yes/no", Required = true },
                            new ScreeningQuestion { QuestionId = "q3_clean_arch", Label = "Explain your experience with Clean Architecture.", Type = "text", Required = false }
                        }
                    },
                    new Job
                    {
                        CompanyId = seededCompany.Id,
                        Title = "Frontend Developer",
                        Description = "We are seeking a Frontend Developer to build clean, pixel-perfect user interfaces using React, TypeScript, and modern component libraries. You will work closely with product managers and backend engineers.",
                        Requirements = "Proficient in React, JavaScript/TypeScript, CSS/HTML. Experience with responsive layouts, state management, and API integration.",
                        Location = "Bengaluru, India",
                        JobType = "Full-time",
                        SalaryRange = "8-12 LPA",
                        RemoteType = "Hybrid",
                        Status = JobStatus.Active,
                        ScreeningQuestions = new List<ScreeningQuestion>
                        {
                            new ScreeningQuestion { QuestionId = "q1_react_exp", Label = "Years of experience with React?", Type = "numeric", Required = true },
                            new ScreeningQuestion { QuestionId = "q2_styling", Label = "What is your primary styling preference (e.g. Tailwind, CSS modules)?", Type = "single-select", Required = true }
                        }
                    },
                    new Job
                    {
                        CompanyId = seededCompany.Id,
                        Title = "Product Manager",
                        Description = "We are hiring a Product Manager to define the product vision, roadmap, and execution strategy for our recruitment platform. You will collaborate with engineering, design, and marketing teams.",
                        Requirements = "Proven experience managing B2B SaaS products. Excellent communication, project management, and analytical skills.",
                        Location = "San Francisco, CA",
                        JobType = "Full-time",
                        SalaryRange = "120k-150k USD",
                        RemoteType = "Onsite",
                        Status = JobStatus.Draft,
                        ScreeningQuestions = new List<ScreeningQuestion>
                        {
                            new ScreeningQuestion { QuestionId = "q1_pm_exp", Label = "How many years of PM experience do you have?", Type = "numeric", Required = true }
                        }
                    },
                    new Job
                    {
                        CompanyId = seededCompany.Id,
                        Title = "Data Scientist",
                        Description = "We are looking for a Data Scientist to build predictive models, run experiments, and uncover insights from recruitment trends.",
                        Requirements = "Strong programming in Python/R. Experience with machine learning libraries, SQL, and data visualization tools.",
                        Location = "London, UK",
                        JobType = "Contract",
                        SalaryRange = "400-500 GBP/day",
                        RemoteType = "Remote",
                        Status = JobStatus.Expired,
                        ScreeningQuestions = new List<ScreeningQuestion>
                        {
                            new ScreeningQuestion { QuestionId = "q1_portfolio", Label = "Please upload your portfolio or research papers.", Type = "file upload", Required = true },
                            new ScreeningQuestion { QuestionId = "q2_tools", Label = "Which data science tools are you proficient in?", Type = "multi-select", Required = true }
                        }
                    }
                };

                db.Jobs.AddRange(jobs);
                await db.SaveChangesAsync();
            }

            await db.SaveChangesAsync();
        }
    }
}
