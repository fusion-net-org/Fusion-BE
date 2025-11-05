
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class CompanySubscriptionAssigmentRepository : GenericRepository<CompanySubscriptionAssignment>, ICompanySubscriptionAssigmentRepository
    {
        private readonly FusionDbContext _context;
        public CompanySubscriptionAssigmentRepository(FusionDbContext context) : base(context)
        {
         _context = context;
        }

        public async Task<CompanySubscriptionAssignment> CreateCompanySubscriptionAsync(Guid ownerId, CompanySubscriptionAssignment req)
        {
            var owner = _context.UserSubscriptions
                 .AsNoTracking()
                 .Where(p => p.UserId == ownerId);
            if (owner == null)
                throw CustomExceptionFactory.CreateForbiddenError();

            var companySubscriptionAssignment = new CompanySubscriptionAssignment
            {
                CompanyMemberId = req.CompanyMemberId,
                UserSubscriptionId = req.UserSubscriptionId,
                CodeTransaction = req.CodeTransaction,
                IsEnabled = req.IsEnabled,
                AssignedAt = DateTime.UtcNow,
                RevokedAt = null,
            };

            await _context.CompanySubscriptionAssignments.AddAsync(companySubscriptionAssignment);
            await _context.SaveChangesAsync();

            return companySubscriptionAssignment;
            
        }

        public async Task<bool> DeleteCompanySubscriptionAsync(Guid ownerId, CompanySubscriptionAssignment req)
        {
            var owner = _context.UserSubscriptions
               .AsNoTracking()
               .Where(p => p.UserId == ownerId);
            if (owner == null)
                throw CustomExceptionFactory.CreateForbiddenError();

            _context.CompanySubscriptionAssignments.Remove(req);
            await _context.SaveChangesAsync();

            return true;
        }

        public async Task<CompanySubscriptionAssignment> UpdateCompanySubscriptionAsync(Guid ownerId, CompanySubscriptionAssignment req)
        {
            var owner = _context.UserSubscriptions
                .AsNoTracking()
                .Where(p => p.UserId == ownerId);
            if (owner == null)
                throw CustomExceptionFactory.CreateForbiddenError();

            var companySubscriptionAssignment = new CompanySubscriptionAssignment
            {
                CompanyMemberId = req.CompanyMemberId,
                UserSubscriptionId = req.UserSubscriptionId,
                CodeTransaction = req.CodeTransaction,
                IsEnabled = req.IsEnabled,
                AssignedAt = DateTime.UtcNow,
                RevokedAt = null,
            };

            _context.CompanySubscriptionAssignments.Update(companySubscriptionAssignment);
            await _context.SaveChangesAsync();

            return companySubscriptionAssignment;
        }
    }
}
