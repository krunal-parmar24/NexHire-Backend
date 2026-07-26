using Microsoft.EntityFrameworkCore;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Persistence
{
    public class NexHireDbContext : DbContext
    {
        public NexHireDbContext(DbContextOptions<NexHireDbContext> options)
            : base(options)
        {
        }

        public DbSet<User> Users { get; set; } = null!;
        public DbSet<Company> Companies { get; set; } = null!;
        public DbSet<Job> Jobs { get; set; } = null!;
        public DbSet<SavedJob> SavedJobs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Entity<User>(b =>
            {
                b.HasKey(u => u.Id);
                b.HasIndex(u => u.Email).IsUnique();
                b.Property(u => u.Email).IsRequired();
                b.Property(u => u.PasswordHash).IsRequired();
                
                b.OwnsOne(u => u.Profile, p =>
                {
                    p.ToJson("profile");
                });
            });

            modelBuilder.Entity<Company>(b =>
            {
                b.HasKey(c => c.Id);
                b.Property(c => c.Name).IsRequired();
            });

            modelBuilder.Entity<Job>(b =>
            {
                b.HasKey(j => j.Id);
                b.Property(j => j.Title).IsRequired();
                b.Property(j => j.Description).IsRequired();
                b.Property(j => j.Requirements).IsRequired();
                b.Property(j => j.Location).IsRequired();
                b.Property(j => j.JobType).IsRequired();
                b.Property(j => j.RemoteType).IsRequired();
                
                b.OwnsMany(j => j.ScreeningQuestions, sq =>
                {
                    sq.ToJson("screening_questions");
                    sq.Property(x => x.QuestionId).HasJsonPropertyName("id");
                });

                b.HasOne(j => j.Company)
                    .WithMany()
                    .HasForeignKey(j => j.CompanyId)
                    .OnDelete(DeleteBehavior.Cascade);
            });

            modelBuilder.Entity<SavedJob>(b =>
            {
                b.HasKey(sj => new { sj.UserId, sj.JobId });

                b.HasOne(sj => sj.User)
                    .WithMany()
                    .HasForeignKey(sj => sj.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                b.HasOne(sj => sj.Job)
                    .WithMany()
                    .HasForeignKey(sj => sj.JobId)
                    .OnDelete(DeleteBehavior.Cascade);
            });
        }
    }
}
