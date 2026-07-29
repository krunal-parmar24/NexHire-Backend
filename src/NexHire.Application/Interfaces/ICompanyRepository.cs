using System.Threading.Tasks;
using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces
{
    /// <summary>Persistence abstraction for recruiter company records.</summary>
    public interface ICompanyRepository
    {
        /// <summary>Persists a new company.</summary>
        Task AddAsync(Company company);
    }
}
