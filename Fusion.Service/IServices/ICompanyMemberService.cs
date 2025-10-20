using Fusion.Repository.Bases.Page;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.Companies.Responses;
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

        Task<PagedResult<CompanyMemberResponse>> GetPagedCompanyMemberByCompanyIdAsync(Guid companyId, string mail, PagedRequest request, CancellationToken token = default);

        Task<CompanyMemberResponse?> FiredMemberFromCompany(string terminatorEmail, string firedMemberMail, Guid companyId, CancellationToken token = default);
    }
}
