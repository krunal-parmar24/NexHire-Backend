using System;
using System.Threading.Tasks;
using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces
{
    /// <summary>Persistence abstraction for user accounts and profiles.</summary>
    public interface IUserRepository
    {
        /// <summary>Gets a user by id.</summary>
        Task<User?> GetByIdAsync(Guid id);

        /// <summary>Gets a user by email, or <c>null</c> if no account is registered with that email.</summary>
        Task<User?> GetByEmailAsync(string email);

        /// <summary>Persists a newly registered user.</summary>
        Task AddAsync(User user);

        /// <summary>Persists changes to an existing user.</summary>
        Task UpdateAsync(User user);
    }
}
