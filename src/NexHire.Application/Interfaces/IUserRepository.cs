using System;
using System.Threading.Tasks;
using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces
{
    public interface IUserRepository
    {
        Task<User?> GetByIdAsync(Guid id);
        Task UpdateAsync(User user);
    }
}
