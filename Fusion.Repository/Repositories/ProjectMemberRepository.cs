using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.ProjectMember;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.Design;
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
                && ((pm.Project!.IsHired && pm.Project.CompanyRequestId == companyId)
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
        public async Task<PagedResult<Project>> GetProjectsByMemberAsync(Guid companyId, Guid userId, ProjectMemberSearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.Projects
                .Include(p => p.ProjectMembers)
                .ThenInclude(pm => pm.User)
                .Where(p => (p.CompanyId == companyId || p.CompanyRequestId == companyId)
                            && p.ProjectMembers.Any(pm => pm.UserId == userId))
                .AsQueryable();

            // Filter ProjectName
            if (!string.IsNullOrWhiteSpace(request.ProjectNameOrCode))
            {
                var keyword = request.ProjectNameOrCode.Trim().ToLower();

                query = query.Where(p =>
                    (p.Name ?? "").ToLower().Contains(keyword) ||
                    (p.Code ?? "").ToLower().Contains(keyword)
                );
            }


            //Filter Status
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(p => (p.Status!.ToLower() == request.Status!.ToLower()));
            }
            // Filter StartDate
            if (request.StartDate.HasValue)
            {
                var from = DateOnly.FromDateTime(request.StartDate.Value);
                query = query.Where(p => p.StartDate >= from);
            }

            // Filter EndDate
            if (request.EndDate.HasValue)
            {
                var to = DateOnly.FromDateTime(request.EndDate.Value);
                query = query.Where(p => p.EndDate <= to);
            }

            return await query.ToPagedResultAsync(request, cancellationToken);
        }
        public async Task AddIfNotExistsAsync(Guid projectId, Guid userId, bool isPartner, bool isViewAll, CancellationToken ct = default)
        {
            var exists = await _context.ProjectMembers.AnyAsync(pm => pm.ProjectId == projectId && pm.UserId == userId, ct);
            if (exists) return;

            await _context.ProjectMembers.AddAsync(new ProjectMember
            {
                ProjectId = projectId,
                UserId = userId,
                IsPartner = isPartner,
                IsViewAll = isViewAll,
                JoinedAt = DateTime.UtcNow
            }, ct);
        }
        public async Task<PagedResult<Project>> GetAllProjectsByMemberIdAsync(Guid userId, ProjectMemberSearchRequest request, CancellationToken cancellationToken = default)
        {
            var query = _context.Projects
                .Include(p => p.ProjectMembers)
                .ThenInclude(pm => pm.User)
                .Where(p => p.ProjectMembers.Any(pm => pm.UserId == userId))
                .AsQueryable();

            // Filter ProjectName
            if (!string.IsNullOrWhiteSpace(request.ProjectNameOrCode))
            {
                var keyword = request.ProjectNameOrCode.Trim().ToLower();

                query = query.Where(p =>
                    (p.Name ?? "").ToLower().Contains(keyword) ||
                    (p.Code ?? "").ToLower().Contains(keyword)
                );
            }


            //Filter Status
            if (!string.IsNullOrWhiteSpace(request.Status))
            {
                query = query.Where(p => (p.Status!.ToLower() == request.Status!.ToLower()));
            }
            // Filter StartDate
            if (request.StartDate.HasValue)
            {
                var from = DateOnly.FromDateTime(request.StartDate.Value);
                query = query.Where(p => p.StartDate >= from);
            }

            // Filter EndDate
            if (request.EndDate.HasValue)
            {
                var to = DateOnly.FromDateTime(request.EndDate.Value);
                query = query.Where(p => p.EndDate <= to);
            }

            return await query.ToPagedResultAsync(request, cancellationToken);
        }
        public Task<bool> UserBelongsToCompanyAsync(Guid userId, Guid companyId, CancellationToken ct = default)
            => _context.CompanyMembers.AnyAsync(cm =>
                    cm.CompanyId == companyId &&
                    cm.UserId == userId &&
                    cm.Status == "Active" &&
                    !(cm.IsDeleted ?? false), ct);
    }
}
