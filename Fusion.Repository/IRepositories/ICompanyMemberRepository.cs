using Fusion.Repository.Bases.Page;
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
        Task<CompanyMember?> InviteMemberToCompany(string inviterEmail, Guid inviteeMemberId, Guid companyId, CancellationToken token = default);

        Task<CompanyMember?> GetCompanyMemberByIdAsync(long id, CancellationToken token = default);

        Task<CompanyMember?> AddCompanyMemberAsync(CompanyMember companyMember, CancellationToken token = default);

        Task<PagedResult<CompanyMember>> GetPagedCompanyMemberByCompanyIdAsync(Guid companyId, string mail, PagedRequest request, CancellationToken token = default);

        Task<CompanyMember?> FiredMemberFromCompany(string terminatorEmail, Guid firedMemberId, Guid companyId, CancellationToken token = default);
    }
}
