using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectMember;
using Fusion.Service.ViewModels.ProjectMembers.Responses;

namespace Fusion.Service.IServices
{
    public interface IProjectMemberService
    {
        Task<PagedResult<MemberProjectListResponse>> GetProjectsByMemberAsync(Guid companyId, Guid userId, ProjectMemberSearchRequest request, CancellationToken cancellationToken = default);
    }
}
