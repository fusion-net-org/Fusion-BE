using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Project;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.IRepositories
{
    public interface IProjectRepository : IGenericRepository<Project>
    {
        Task<Project> CreateProjectAsync(Guid userId, Project request, CancellationToken cancellationToken = default);
        Task<PagedResult<Project>> GetAllProjectAsync(ProjectSearchRequest req, CancellationToken ct = default);
        Task<Project?> GetProjectDetailAsync(Guid id, CancellationToken ct = default);
        Task<PagedResult<Project>> GetProjectByMemberIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default);
        Task<PagedResult<Project>> GetProjectByActorIdAsync(Guid userId, ProjectSearchRequest req, CancellationToken ct = default);
        //Task<PagedResult<Project>> GetProjectByStatusAsync(string status, ProjectSearchRequest req, CancellationToken ct = default);
        Task<(int Todo, int Cancel, int Finish)> GetCountProjectByStatusAsync(CancellationToken ct = default);
        Task<int> GetAllProjectCountAsync(CancellationToken cancellationToken = default);

    }
}
