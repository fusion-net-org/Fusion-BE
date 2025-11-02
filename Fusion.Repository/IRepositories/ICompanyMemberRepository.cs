using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company_Member;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static Fusion.Repository.Repositories.CompanyMemberRepository;

namespace Fusion.Repository.IRepositories
{
    public interface ICompanyMemberRepository : IGenericRepository<CompanyMember>
    {
        Task<CompanyMember?> InviteMemberToCompany(string inviterEmail, string inviteeMemberMail, Guid companyId, CancellationToken token = default);

        Task<CompanyMember?> GetCompanyMemberByIdAsync(long id, CancellationToken token = default);

        Task<CompanyMember?> AddCompanyMemberAsync(CompanyMember companyMember, CancellationToken token = default);

        Task<PagedResult<CompanyMember>> GetPagedCompanyMemberByCompanyIdAsync(Guid companyId, string mail, CompanyMemberPagedSearchRequest request, CancellationToken token = default);

        Task<PagedResult<CompanyMember>> GetPagedCompanyMemberAsync(CompanyMemberPagedSearchAdminRequest request, CancellationToken token);

        Task<CompanyMember?> FiredMemberFromCompany(string terminatorEmail, string firedMemberMail, string reason, Guid companyId, CancellationToken token = default);

        Task<CompanyMember?> AcceptJoinMemberToCompany(Guid inviteeMemberId, Guid companyId, CancellationToken token =  default);

        Task<CompanyMember?> RejectJoinMemberToCompany(Guid inviteeMemberId, Guid companyId, CancellationToken token = default);

        Task<CompanyMember?> RemoveMemberFromCompany(string terminatorEmail, Guid userId, Guid companyId, CancellationToken token = default);

        Task<List<CompanyMember>> GetMembersByStatus(Guid companyId, string status, CancellationToken token = default);

        Task<Dictionary<string, int>> GetSummaryStatusByCompanyId(Guid companyId, CancellationToken token = default);

        Task<List<UserRole?>> AddRoleForMemberInCompany(Guid companyId, List<int> roleIds, Guid memberId, string inviterEmail, CancellationToken token =  default);
        Task<CompanyMember?> GetCompanyMemberByCompanyIdAndUserIdAsync(Guid companyId, Guid userId, CancellationToken token = default);

        Task<Dictionary<Guid, UserRoleLite>> GetUserRoleMapInCompanyAsync(
     Guid companyId, IEnumerable<Guid> userIds, CancellationToken token = default);
    }
}
