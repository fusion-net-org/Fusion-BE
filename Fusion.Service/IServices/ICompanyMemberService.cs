using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company_Member;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.UserRole.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
    public interface ICompanyMemberService
    {
        Task<CompanyMemberResponse?> InviteMemberToCompany(string inviterEmail, string inviteeMemberMail, Guid CompanyId, CancellationToken token = default);

        Task<PagedResult<CompanyMemberResponse>> GetPagedCompanyMemberByCompanyIdAsync(Guid companyId, string mail, CompanyMemberPagedSearchRequest request, CancellationToken token = default);

        Task<PagedResult<CompanyMemberResponse>> GetPagedCompanyMemberAsync(CompanyMemberPagedSearchAdminRequest request, CancellationToken token);

        Task<CompanyMemberResponse?> FiredMemberFromCompany(string terminatorEmail, string firedMemberMail, string reason, Guid companyId, CancellationToken token = default);

        Task<CompanyMemberResponse?> AcceptJoinMemberToCompany(string tokenConfirm, CancellationToken cancellationToken = default);

        Task<CompanyMemberResponse?> RejectJoinMemberToCompany(string tokenConfirm, CancellationToken cancellationToken = default);

        Task<CompanyMemberResponse?> RemoveMemberFromCompany(string terminatorEmail, Guid userId, Guid companyId, CancellationToken token = default);
        Task<List<CompanyMemberResponse>> GetMembersByStatus(Guid companyId, string status, CancellationToken token = default);

        Task<Dictionary<string, int>> GetSummaryStatusByCompanyId(Guid companyId, CancellationToken token = default);

        Task<AddMemberRoleInCompanyResponse?> AddRoleForMemberInCompany(Guid companyId, List<int> roleIds, Guid memberId, string inviterEmail, CancellationToken token = default);
        Task<CompanyMemberResponse?> GetCompanyMemberByCompanyIdAndUserIdAsync(Guid companyId, Guid userId, CancellationToken token = default);
        Task<PagedResult<CompanyMemberResponseV2>> GetCompanyMemberByUserIdAsync(Guid userId,CompanyMemberPagedRequest request,CancellationToken token = default);

        Task<CompanyMemberResponse?> AcceptJoinMemberById(long memberId, CancellationToken token = default);
        Task<CompanyMemberResponse?> RejectJoinMemberById(long memberId, CancellationToken token = default);
    }

}
