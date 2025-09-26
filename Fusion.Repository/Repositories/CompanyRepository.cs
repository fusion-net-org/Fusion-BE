using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        private readonly FusionDbContext _context;
        public CompanyRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Guid?> GetCompanyIdByUserId(Guid userId)
        {
            var company = await _context.Companies
                .FirstOrDefaultAsync(x => x.OwnerUserId == userId);

            return company?.Id;
        }


        public async Task<string> GetCompanyNameByGuid(Guid company)
        {
            var companyName = await _context.Companies.FindAsync(company);
            return companyName?.Name;
        }

        public async Task<string> GetMailCompanyByGuid(Guid companyId)
        {
            var company = await _context.Companies.FindAsync(companyId);
            return company?.Email;
        }

    }
}
