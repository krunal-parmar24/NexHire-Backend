using System;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using NexHire.Application.Interfaces;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Persistence.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly NexHireDbContext _db;

        public UserRepository(NexHireDbContext db)
        {
            _db = db;
        }

        public async Task<User?> GetByIdAsync(Guid id)
        {
            return await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task UpdateAsync(User user)
        {
            _db.Users.Update(user);
            await _db.SaveChangesAsync();
        }
    }
}
