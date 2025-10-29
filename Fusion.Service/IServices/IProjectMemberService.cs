using Fusion.Service.ViewModels.ProjectMembers.Responses;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Service.IServices
{
    public interface IProjectMemberService
    {
        Task<MemberProjectListResponse> GetProjectsByMemberAsync(Guid companyId, Guid userId);
    }
}
