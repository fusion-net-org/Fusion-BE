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
        Task<bool?> InviteMemberToCompany(string inviterEmail, Guid inviteeMemberId, Guid CompanyId, CancellationToken token = default);
        Task<CompanyMemberResponse?> JoinMemberToCompany(string tokenConfirm, CancellationToken token = default);
    }
}
