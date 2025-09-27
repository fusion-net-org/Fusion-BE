using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    internal class CompanyRepository : GenericRepository<Company>, ICompanyRepository
    {
        private readonly FusionDbContext _context;

        public CompanyRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<PagedResult<Company>> GetPagedCompaniesAsync(CompanyPagedSearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = _dbSet.Include(x => x.CompanyMembers).Include(x => x.OwnerUser).AsQueryable();

            // search
            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(u => (u.Name ?? "").Contains(request.Name));
            }

            if (!string.IsNullOrWhiteSpace(request.TaxCode))
            {
                query = query.Where(u => (u.TaxCode ?? "").Contains(request.TaxCode));
            }

            // dùng extension để phân trang + sort
            return await query.ToPagedResultAsync(request, cancellationToken);
        }

    }
}
