using NexHire.Infrastructure.Persistence;
using NexHire.Domain.Entities;
using NexHire.Domain.Enums;
using NexHire.Infrastructure.Services;
using Microsoft.EntityFrameworkCore;

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
                }
            }

            await db.SaveChangesAsync();
        }
    }
}
