using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Fusion.Repository.Repositories
{
    public class ProjectMemberRepository : GenericRepository<ProjectMember>, IProjectMemberRepository
    {
        private readonly FusionDbContext _context;

        public ProjectMemberRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<int> GetTotalProjectsForMemberInCompanyAsync(Guid memberId, Guid companyId, CancellationToken cancellationToken = default)
        {
            var totalProjects = await _context.ProjectMembers
                .Include(x => x.Project)
                .Where(pm =>
                pm.UserId == memberId
                && ((pm.Project!.IsHired && pm.Project.CompanyHiredId == companyId)
                || (!pm.Project.IsHired && pm.Project.CompanyId == companyId)))
                .Select(pm => pm.ProjectId).Distinct().CountAsync(cancellationToken);

            return totalProjects;
        }

        public async Task<int> GetTotalProjectsForMemberAsync(Guid memberId, CancellationToken cancellationToken = default)
        {
            var totalProjects = await _context.ProjectMembers
                .Include(x => x.Project)
                .Where(pm => pm.UserId == memberId)
                .Select(pm => pm.ProjectId)
                .Distinct()
                .CountAsync(cancellationToken);

            return totalProjects;
        }

        public async Task<List<Project>> GetProjectsByMemberAsync(Guid companyId, Guid userId)
        {
            return await _context.Projects
                .Include(p => p.ProjectMembers)
                    .ThenInclude(pm => pm.User)
                .Where(p =>
                    (p.CompanyId == companyId || p.CompanyHiredId == companyId) &&
                    p.ProjectMembers.Any(pm => pm.UserId == userId))
                .ToListAsync();
        }


    }
}
