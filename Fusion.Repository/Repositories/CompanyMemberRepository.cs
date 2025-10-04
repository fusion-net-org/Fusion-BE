using Fusion.Repository.Bases.Exceptions;
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

        public async Task<CompanyMember?> AddCompanyMemberAsync(CompanyMember companyMember, CancellationToken token)
        {
            var newCompany = await _context.CompanyMembers.AddAsync(companyMember, token);
            await _context.SaveChangesAsync(token);

            return newCompany.Entity;
        }

        public async Task<bool?> InviteMemberToCompany(string inviterEmail, Guid inviteeMemberId, Guid companyId, CancellationToken token)
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
                Status = false,
            };

            await _context.CompanyMembers.AddAsync(companyMember, token);
            await _context.SaveChangesAsync(token);

            return true;
        }

        public async Task<CompanyMember?> JoinMemberToCompany(Guid inviteeMemberId, Guid companyId, CancellationToken token)
        {
            var companyMember = await _context.CompanyMembers
                .Include(x => x.User)
                .Include(x => x.Company)
                .SingleOrDefaultAsync(cm => cm.UserId == inviteeMemberId && cm.CompanyId == companyId, token);

            if (companyMember == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Member company does not existed"));

            companyMember.JoinedAt = DateTime.UtcNow.AddHours(7);
            companyMember.Status = true;

            _context.CompanyMembers.Update(companyMember);
            await _context.SaveChangesAsync(token);

            return companyMember;

        }
    }
}
