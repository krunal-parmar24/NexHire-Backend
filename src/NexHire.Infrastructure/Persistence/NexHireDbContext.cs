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
        }
    }
}
