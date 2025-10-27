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
    }
}
