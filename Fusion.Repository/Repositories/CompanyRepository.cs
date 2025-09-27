using Azure.Core;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Data;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.Design;
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

        public async Task<Company?> AddCompanyAsync(User user, string image_company, Company new_company, CancellationToken cancellationToken)
        {
            new_company.ImageCompany = image_company;
            new_company.OwnerUserId = user.Id;
            new_company.IsDeleted = false;
            new_company.CreateAt = DateTime.UtcNow.AddHours(7);

            var company = await _context.Companies.AddAsync(new_company);
            await _context.SaveChangesAsync();
            return company.Entity;
        }

        public async Task<Company?> UpdateCompanyAsync(string image_company, Guid companyId, Company update_company, CancellationToken cancellationToken = default)
        {
            var existed_company = await _context.Companies.FindAsync(companyId);

            existed_company.Name = update_company.Name ?? existed_company.Name;
            existed_company.TaxCode = update_company.TaxCode ?? existed_company.TaxCode;
            existed_company.Detail = update_company.Detail ?? existed_company.Detail;
            existed_company.Email = update_company.Email ?? existed_company.Email;
            existed_company.ImageCompany = image_company;
            existed_company.UpdateAt = DateTime.UtcNow.AddHours(7);

            var company = _context.Companies.Update(existed_company);

            await _context.SaveChangesAsync(cancellationToken);
            return company.Entity;
        }

        public async Task<bool?> DeleteCompanyAsync(Company company, CancellationToken cancellationToken = default)
        {
            company.IsDeleted = true;
            _context.Companies.Update(company);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }

        public async Task<Company?> GetCompanyByTaxCode(string taxcode)
        {
            var company = await _context.Companies
                .SingleOrDefaultAsync(x => x.TaxCode == taxcode);

            return company;
        }

        public async Task<Company?> GetCompanyByEmail(string email)
        {
            var company = await _context.Companies
                .SingleOrDefaultAsync(x => x.Email == email);

            return company;
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

        public async Task<Company?> GetCompanyByIdAsync(Guid Id)
        {
            return await _context.Companies.Include(x => x.OwnerUser).SingleOrDefaultAsync(x => x.Id == Id);
        }

        
    }
}
