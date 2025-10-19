using Azure.Core;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Dynamic.Core.Tokenizer;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public class CompanyMemberRepository : GenericRepository<CompanyMember>, ICompanyMemberRepository
    {
        private readonly FusionDbContext _context;

        public CompanyMemberRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<CompanyMember?> AddCompanyMemberAsync(CompanyMember companyMember, CancellationToken token = default)
        {
            var newCompany = await _context.CompanyMembers.AddAsync(companyMember, token);
            await _context.SaveChangesAsync(token);

            return newCompany.Entity;
        }

        public async Task<CompanyMember?> GetCompanyMemberByIdAsync(long id, CancellationToken token = default)
        {
            return await _context.CompanyMembers
                .Include(cm => cm.Company)
                    .ThenInclude(c => c.OwnerUser)
                .Include(cm => cm.User)
                .SingleOrDefaultAsync(cm => cm.Id == id, token);
        }
 
        public async Task<CompanyMember?> InviteMemberToCompany(string inviterEmail, Guid inviteeMemberId, Guid companyId, CancellationToken token)
        {
            var owner_user = await _context.Users.SingleOrDefaultAsync(x => x.Email == inviterEmail, token);
            if (owner_user == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.INVALID_INPUT.FormatMessage("Email do not existed! "));

            var company = await _context.Companies.FindAsync(companyId, token);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.INVALID_INPUT.FormatMessage("Company does not exist!"));


            if (company.OwnerUserId != owner_user.Id)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.INVALID_INPUT.FormatMessage($"{company.Name} does not belong to {owner_user.UserName}"));

            var member = await _context.Users.FindAsync(inviteeMemberId, token);
            if (member == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.INVALID_INPUT.FormatMessage("Member does not register to the system!"));

            var alreadyInCompany = await _context.CompanyMembers
                  .AnyAsync(cm => cm.CompanyId == companyId && cm.UserId == inviteeMemberId, token);

            if (alreadyInCompany)
                throw CustomExceptionFactory.CreateBadRequestError(
                    ResponseMessages.DUPLICATE.FormatMessage("Member already belongs to this company!"));

            var companyMember = new CompanyMember
            {
                CompanyId = companyId,
                UserId = inviteeMemberId,
                Status = true,
                JoinedAt = DateTime.UtcNow.AddHours(7),
            };

            await _context.CompanyMembers.AddAsync(companyMember, token);
            await _context.SaveChangesAsync(token);

            return companyMember;
        }

        public async Task<PagedResult<CompanyMember>> GetPagedCompanyMemberByCompanyIdAsync(Guid companyId, string mail, PagedRequest request, CancellationToken token)
        {
            var company = await _context.Companies.FindAsync(companyId, token);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company"));

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == mail);
            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("User"));

            var isMember = await _context.CompanyMembers.SingleOrDefaultAsync(cm => cm.UserId == user.Id && cm.CompanyId == companyId);
            if (isMember == null)
                throw CustomExceptionFactory.CreateNotFoundError(ResponseMessages.FORBIDDEN.FormatMessage($"User can not access to {company.Name}"));

            var query = _context.CompanyMembers
                .Include(x => x.User)
                .Include(x => x.Company)
                    .ThenInclude(c => c.OwnerUser)
                .AsQueryable();


            query = query.Where(cm => cm.CompanyId == companyId).OrderByDescending(cm => cm.JoinedAt);

            return await query.ToPagedResultAsync(request, token);
        }

        public async Task<CompanyMember?> FiredMemberFromCompany(string terminatorEmail, Guid firedMemberId, Guid companyId, CancellationToken token)
        {
            var company = await _context.Companies
              .Include(c => c.OwnerUser)
              .SingleOrDefaultAsync(c => c.Id == companyId && c.OwnerUser.Email == terminatorEmail);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage($"{terminatorEmail}"));


            var companyMember = await _context.CompanyMembers
                .SingleOrDefaultAsync(cm => 
                    cm.CompanyId == companyId
                    && cm.UserId == firedMemberId, token);

            if (companyMember == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("User in Company"));

            companyMember.Status = false;
            await _context.SaveChangesAsync(token);

            return companyMember;
        }
    }
}
