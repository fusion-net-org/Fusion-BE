using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface ICompanyMemberRepository : IGenericRepository<CompanyMember>
    {
        Task<bool?> InviteMemberToCompany(string inviterEmail, Guid inviteeMemberId, Guid companyId, CancellationToken token = default);

        Task<CompanyMember?> JoinMemberToCompany(Guid inviteeMemberId, Guid companyId, CancellationToken token = default);

        Task<CompanyMember?> AddCompanyMemberAsync(CompanyMember companyMember, CancellationToken token = default);
    }
}
