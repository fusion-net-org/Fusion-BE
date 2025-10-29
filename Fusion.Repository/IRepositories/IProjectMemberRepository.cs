using Fusion.Repository.Data;
using Fusion.Repository.Entities;
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

        Task<List<Project>> GetProjectsByMemberAsync(Guid companyId, Guid userId);
    }
}
