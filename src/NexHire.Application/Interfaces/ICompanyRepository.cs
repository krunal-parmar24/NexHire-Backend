using System.Threading.Tasks;
using NexHire.Domain.Entities;

namespace NexHire.Application.Interfaces
{
    public interface ICompanyRepository
    {
        Task AddAsync(Company company);
    }
}
