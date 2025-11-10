using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectMember;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.ViewModels;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface IProjectMemberRepository: IGenericRepository<ProjectMember>
    {
        Task<int> GetTotalProjectsForMemberInCompanyAsync(Guid memberId, Guid companyId, CancellationToken cancellationToken = default);
        Task<int> GetTotalProjectsForMemberAsync(Guid memberId, CancellationToken cancellationToken = default);
        Task<PagedResult<Project>> GetProjectsByMemberAsync(Guid companyId, Guid userId, ProjectMemberSearchRequest request, CancellationToken cancellationToken = default);
        Task<PagedResult<Project>> GetAllProjectsByMemberIdAsync(Guid userId, ProjectMemberSearchRequest request, CancellationToken cancellationToken = default);
        Task AddIfNotExistsAsync(Guid projectId, Guid userId, bool isPartner, bool isViewAll, CancellationToken ct = default);
        Task<bool> UserBelongsToCompanyAsync(Guid userId, Guid companyId, CancellationToken ct = default);

        Task<MemberPerformanceStats> GetMemberPerformanceAsync(Guid userId, Guid companyId, CancellationToken token = default);

    }
}
