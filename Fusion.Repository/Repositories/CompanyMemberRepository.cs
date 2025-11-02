using Azure.Core;
using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company_Member;
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
        public sealed class UserRoleLite
        {
            public int? RoleId { get; set; }
            public string? RoleName { get; set; }
        }
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

        public async Task<CompanyMember?> InviteMemberToCompany(string inviterEmail, string inviteeMemberMail, Guid companyId, CancellationToken token)
        {
            CompanyMember companyMember = null;

            var owner_user = await _context.Users.SingleOrDefaultAsync(x => x.Email == inviterEmail, token);
            if (owner_user == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError("Email do not existed! ");

            var company = await _context.Companies.FindAsync(companyId, token);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError("Company does not exist!");


            if (company.OwnerUserId != owner_user.Id)
                throw CustomExceptionFactory.CreateNotFoundError($"{company.Name} does not belong to {owner_user.UserName}");

            var member = await _context.Users.SingleOrDefaultAsync(u => u.Email == inviteeMemberMail, token);
            if (member == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError("Member does not register to the system!");

            var alreadyInCompany = await _context.CompanyMembers
                  .SingleOrDefaultAsync(cm => cm.CompanyId == companyId && cm.UserId == member.Id && cm.IsDeleted == false, token);

            if (alreadyInCompany != null)
            {
                if (alreadyInCompany.Status == "Active")
                {
                    throw CustomExceptionFactory.CreateBadRequestError("Member already belongs to this company!");
                }
                else
                {
                    alreadyInCompany.Status = "Pending";
                    companyMember = alreadyInCompany;
                }
            }
            else
            {
                companyMember = new CompanyMember
                {
                    CompanyId = companyId,
                    UserId = member.Id,
                    Status = "Pending",
                    IsDeleted = false,
                    JoinedAt = DateTime.UtcNow.AddHours(7),
                };

                await _context.CompanyMembers.AddAsync(companyMember, token);
            }


            await _context.SaveChangesAsync(token);

            return await _context.CompanyMembers
                .Include(cm => cm.Company)
                    .ThenInclude(c => c.OwnerUser)
                .Include(cm => cm.User)
                .SingleOrDefaultAsync(cm => cm.Id == companyMember.Id, token);
        }
        public async Task<Dictionary<Guid, UserRoleLite>> GetUserRoleMapInCompanyAsync(
     Guid companyId, IEnumerable<Guid> userIds, CancellationToken token = default)
        {
            var result = new Dictionary<Guid, UserRoleLite>();
            var uidList = (userIds ?? Array.Empty<Guid>()).Distinct().ToList();
            if (uidList.Count == 0) return result;

            var roles = await _context.Roles
                .AsNoTracking()
                .Where(r => (r.CompanyId.HasValue && r.CompanyId.Value == companyId) || r.CompanyId == companyId)
                .ToListAsync(token);
            if (roles.Count == 0) return result;

            var roleMap = new Dictionary<int, string>(roles.Count);
            foreach (var r in roles)
                roleMap[r.Id] = r.RoleName ?? string.Empty;

            var userRoles = await _context.UserRoles
                .AsNoTracking()
                .Where(ur => ur.UserId.HasValue && uidList.Contains(ur.UserId.Value) && ur.RoleId.HasValue)
                .ToListAsync(token);

            foreach (var ur in userRoles)
            {
                if (!ur.UserId.HasValue || !ur.RoleId.HasValue) continue;

                var uid = ur.UserId.Value;    
                var rid = ur.RoleId.Value;    

                if (!roleMap.TryGetValue(rid, out var roleName)) continue;

                if (result.TryGetValue(uid, out var cur))
                {
                    var curId = cur.RoleId ?? int.MaxValue;
                    if (rid < curId) result[uid] = new UserRoleLite { RoleId = rid, RoleName = roleName };
                }
                else
                {
                    result[uid] = new UserRoleLite { RoleId = rid, RoleName = roleName };
                }
            }

            return result;
        }


        public async Task<PagedResult<CompanyMember>> GetPagedCompanyMemberByCompanyIdAsync(Guid companyId, string mail, CompanyMemberPagedSearchRequest request, CancellationToken token)
        {
            var company = await _context.Companies.FindAsync(companyId, token);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError("Company not found");

            var user = await _context.Users.SingleOrDefaultAsync(u => u.Email == mail);
            if (user == null)
                throw CustomExceptionFactory.CreateNotFoundError("User not found");

            var isMember = await _context.CompanyMembers.SingleOrDefaultAsync(cm => cm.UserId == user.Id && cm.CompanyId == companyId);
            if (isMember == null)
                throw CustomExceptionFactory.CreateNotFoundError($"User can not access to {company.Name}");

            var query = _context.CompanyMembers
                .Include(x => x.User)
                .Include(x => x.Company)
                    .ThenInclude(c => c.OwnerUser)
                .AsQueryable();


            query = query.Where(cm => cm.CompanyId == companyId);

            if (!string.IsNullOrEmpty(request.KeyWord))
            {
                var keyword = request.KeyWord.ToLower();

                query = query.Where(u =>
                    (u.User.UserName ?? "").ToLower().Contains(keyword) ||
                    (u.User.Email ?? "").ToLower().Contains(keyword) ||
                    (u.User.Phone ?? "").ToLower().Contains(keyword) ||
                    (u.User.Gender ?? "").ToLower() == keyword.ToLower()
                );
            }

            if (request.DateRange != null)
            {
                var vnTime = DateTime.UtcNow.AddHours(7);

                if (request.DateRange.From.HasValue && request.DateRange.To.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    var to = request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(cm => cm.JoinedAt >= from && cm.JoinedAt <= to);
                }
                else if (request.DateRange.From.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(cm => cm.JoinedAt >= from);
                }
                else if (request.DateRange.To.HasValue)
                {
                    var to = request.DateRange.To.Value.ToDateTime(TimeOnly.MaxValue);
                    query = query.Where(cm => cm.JoinedAt <= to);
                }
            }


            return await query.ToPagedResultAsync(request, token);
        }

        public async Task<PagedResult<CompanyMember>> GetPagedCompanyMemberAsync(CompanyMemberPagedSearchAdminRequest request, CancellationToken token)
        {

            var query = _context.CompanyMembers
                .Include(x => x.User)
                .Include(x => x.Company)
                    .ThenInclude(c => c.OwnerUser)
                .AsQueryable();

            if (!string.IsNullOrEmpty(request.CompanyName))
            {
                query = query.Where(u => (u.Company.Name ?? "").Contains(request.CompanyName));
            }

            if (!string.IsNullOrEmpty(request.MemberName))
            {
                query = query.Where(u => (u.User.UserName ?? "").Contains(request.MemberName));
            }

            if (request.DateRange != null)
            {
                if (request.DateRange.From.HasValue && request.DateRange.To.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    var to = request.DateRange.To.Value
                        .ToDateTime(TimeOnly.MinValue)
                        .AddDays(1)
                        .AddTicks(-1);
                    query = query.Where(cm => cm.JoinedAt >= from && cm.JoinedAt <= to);
                }
                else if (request.DateRange.From.HasValue)
                {
                    var from = request.DateRange.From.Value.ToDateTime(TimeOnly.MinValue);
                    query = query.Where(cm => cm.JoinedAt >= from);
                }
                else if (request.DateRange.To.HasValue)
                {
                    var to = request.DateRange.To.Value
                        .ToDateTime(TimeOnly.MinValue)
                        .AddDays(1)
                        .AddTicks(-1);
                    query = query.Where(cm => cm.JoinedAt <= to);
                }
            }


            return await query.ToPagedResultAsync(request, token);
        }

        public async Task<CompanyMember?> FiredMemberFromCompany(string terminatorEmail, string firedMemberMail, string reason, Guid companyId, CancellationToken token)
        {
            var company = await _context.Companies
              .Include(c => c.OwnerUser)
              .SingleOrDefaultAsync(c => c.Id == companyId && c.OwnerUser.Email == terminatorEmail);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError($"{terminatorEmail} Email not found");


            var companyMember = await _context.CompanyMembers
                .Include(cm => cm.User)
                .Include(cm => cm.Company)
                    .ThenInclude(c => c.OwnerUser)
                .SingleOrDefaultAsync(cm =>
                    cm.CompanyId == companyId
                    && cm.User.Email == firedMemberMail, token);

            if (companyMember == null)
                throw CustomExceptionFactory.CreateNotFoundError("User in Company not found");

            companyMember.Status = "InActive";
            companyMember.Reason = reason;
            await _context.SaveChangesAsync(token);

            return companyMember;
        }

        public async Task<CompanyMember?> RemoveMemberFromCompany(string terminatorEmail, Guid userId, Guid companyId, CancellationToken token)
        {
            var company = await _context.Companies
              .Include(c => c.OwnerUser)
              .SingleOrDefaultAsync(c => c.Id == companyId && c.OwnerUser.Email == terminatorEmail);
            if (company == null)
                throw CustomExceptionFactory.CreateNotFoundError($"{terminatorEmail} Email not found");


            var companyMember = await _context.CompanyMembers.Include(cm => cm.User)
                .SingleOrDefaultAsync(cm =>
                    cm.CompanyId == companyId
                    && cm.UserId == userId && cm.Status == "InActive", token);

            if (companyMember == null)
                throw CustomExceptionFactory.CreateNotFoundError("User in Company not found");

            companyMember.IsDeleted = true;
            await _context.SaveChangesAsync(token);

            return companyMember;
        }

        public async Task<CompanyMember?> AcceptJoinMemberToCompany(Guid inviteeMemberId, Guid companyId, CancellationToken token)
        {
            var companyMember = await _context.CompanyMembers
                .Include(x => x.User)
                .Include(x => x.Company)
                .SingleOrDefaultAsync(cm => cm.UserId == inviteeMemberId && cm.CompanyId == companyId && cm.Status == "Pending" && cm.IsDeleted == false, token);

            if (companyMember == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Member company does not existed"));

            companyMember.JoinedAt = DateTime.UtcNow.AddHours(7);
            companyMember.Status = "Active";
            companyMember.IsDeleted = false;

            _context.CompanyMembers.Update(companyMember);
            await _context.SaveChangesAsync(token);

            return companyMember;

        }

        public async Task<CompanyMember?> RejectJoinMemberToCompany(Guid inviteeMemberId, Guid companyId, CancellationToken token)
        {
            var companyMember = await _context.CompanyMembers
                .Include(x => x.User)
                .Include(x => x.Company)
                .SingleOrDefaultAsync(cm => cm.UserId == inviteeMemberId && cm.CompanyId == companyId && cm.Status == "Pending" && cm.IsDeleted == false, token);

            if (companyMember == null)
                throw CustomExceptionFactory.
                    CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Member company does not existed"));

            companyMember.Status = "InActive";
            companyMember.IsDeleted = false;

            _context.CompanyMembers.Update(companyMember);
            await _context.SaveChangesAsync(token);

            return companyMember;

        }

        public async Task<List<UserRole?>> AddRoleForMemberInCompany(Guid companyId, List<int> roleIds, Guid memberId, string inviterEmail, CancellationToken token)
        {
            var company = await _context.Companies.FindAsync(companyId);
            if (company == null)
                throw CustomExceptionFactory.
                     CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Company is not existed"));

            var member = await _context.Users.FindAsync(memberId);
            if (member == null)
                throw CustomExceptionFactory.
                     CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Member is not existed"));

            var company_member = await _context.CompanyMembers.SingleOrDefaultAsync(x => x.CompanyId == companyId && x.UserId == memberId);
            if (company_member == null)
                throw CustomExceptionFactory.
                     CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Member company does not existed"));

            var inviter = await _context.Users.FirstOrDefaultAsync(u => u.Email == inviterEmail);
            if (inviter == null)
                throw CustomExceptionFactory.CreateNotFoundError(
                    ResponseMessages.NOT_FOUND.FormatMessage("Inviter does not exist"));

            var company_inviter = await _context.CompanyMembers.SingleOrDefaultAsync(x => x.CompanyId == companyId && x.UserId == inviter.Id);
            if (company_inviter == null)
                throw CustomExceptionFactory.
                     CreateNotFoundError(ResponseMessages.NOT_FOUND.FormatMessage("Inivter company does not existed"));

            if (company_inviter.CompanyId != company_member.CompanyId)
                throw CustomExceptionFactory.
                     CreateNotFoundError(ResponseMessages.BAD_REQUEST.FormatMessage("Inivter and Member not in the same company"));

            var addedRoles = new List<UserRole>();

            foreach (var roleId in roleIds)
            {
                var role = await _context.Roles.FirstOrDefaultAsync(r => r.Id == roleId && r.CompanyId == companyId);
                if (role == null)
                    throw CustomExceptionFactory.CreateNotFoundError(
                        ResponseMessages.NOT_FOUND.FormatMessage($"Role {roleId} does not belong to this company"));

                // check if this user already has that role
                var exist = await _context.UserRoles.FirstOrDefaultAsync(x => x.UserId == memberId && x.RoleId == roleId);
                if (exist != null)
                    continue;

                var newUserRole = new UserRole
                {
                    Id = Guid.NewGuid(),
                    UserId = memberId,
                    RoleId = roleId
                };

                await _context.UserRoles.AddAsync(newUserRole, token);
                addedRoles.Add(newUserRole);
            }

            if (addedRoles.Any())
                await _context.SaveChangesAsync(token);

            return addedRoles;
        }

        public async Task<List<CompanyMember>> GetMembersByStatus(Guid companyId, string status, CancellationToken token = default)
        {
            return await _context.CompanyMembers
                .Include(cm => cm.User)
                .Include(cm => cm.Company)
                .Where(cm => cm.CompanyId == companyId && cm.Status == status)
                .ToListAsync(token);
        }

        public async Task<Dictionary<string, int>> GetSummaryStatusByCompanyId(Guid companyId, CancellationToken token = default)
        {
            var query = _context.CompanyMembers
                .Where(cm => cm.CompanyId == companyId);

            var result = await query
                .GroupBy(cm => cm.Status)
                .Select(g => new { Status = g.Key, Count = g.Count() })
                .ToListAsync(token);

            return result.ToDictionary(x => x.Status, x => x.Count);
        }

    }
}
