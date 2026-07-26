using System.Threading.Tasks;
using NexHire.Application.Interfaces;
using NexHire.Domain.Entities;

namespace NexHire.Infrastructure.Persistence.Repositories
{
    public class CompanyRepository : ICompanyRepository
    {
        private readonly NexHireDbContext _db;

        public CompanyRepository(NexHireDbContext db)
        {
            _db = db;
        }

        public async Task AddAsync(Company company)
        {
            _db.Companies.Add(company);
            await _db.SaveChangesAsync();
        }
    }
}
