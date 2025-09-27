using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Data;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using static Microsoft.EntityFrameworkCore.DbLoggerCategory.Database;

namespace Fusion.Repository.Repositories
{
    public class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        private readonly FusionDbContext _context;

        public CompanyRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PagedResult<Company>> GetPagedCompaniesAsync(CompanyPagedSearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbSet
                .Include(x => x.CompanyMembers)
                .Include(x => x.OwnerUser)
                .AsQueryable();

            // search
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(u => (u.Name ?? "").Contains(request.Name));
            }

            if (!string.IsNullOrWhiteSpace(request.TaxCode))
            {
                query = query.Where(u => (u.TaxCode ?? "").Contains(request.TaxCode));
            }

            return await query.ToPagedResultAsync(request, cancellationToken);
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
