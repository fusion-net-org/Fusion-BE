using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class ProjectComponentRepository
        : GenericRepository<ProjectComponent>, IProjectComponentRepository
    {
        private readonly FusionDbContext _context;

        public ProjectComponentRepository(FusionDbContext context)
            : base(context)
        {
            _context = context;
        }

        public async Task<List<ProjectComponent>> CreateManyAsync(
         List<ProjectComponent> components,
         CancellationToken cancellationToken = default)
        {
            await _context.ProjectComponents.AddRangeAsync(components, cancellationToken);
            await _context.SaveChangesAsync(cancellationToken);
            return components;
        }


        public async Task<ProjectComponent?> GetByIdAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            return await _context.ProjectComponents
                .Include(x => x.Project)
                .Include(x => x.ProjectRequest)
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);
        }

        public async Task<List<ProjectComponent>> GetByProjectIdAsync(
            Guid projectId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ProjectComponents
                .Where(x => x.ProjectId == projectId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<List<ProjectComponent>> GetByProjectRequestIdAsync(
            Guid projectRequestId,
            CancellationToken cancellationToken = default)
        {
            return await _context.ProjectComponents
                .Where(x => x.ProjectRequestId == projectRequestId)
                .OrderByDescending(x => x.CreatedAt)
                .ToListAsync(cancellationToken);
        }

        public async Task<ProjectComponent> UpdateAsync(
            ProjectComponent component,
            CancellationToken cancellationToken = default)
        {
            _context.ProjectComponents.Update(component);
            await _context.SaveChangesAsync(cancellationToken);
            return component;
        }

        public async Task<bool> DeleteAsync(
            Guid id,
            CancellationToken cancellationToken = default)
        {
            var component = await _context.ProjectComponents
                .FirstOrDefaultAsync(x => x.Id == id, cancellationToken);

            if (component == null)
                return false;

            _context.ProjectComponents.Remove(component);
            await _context.SaveChangesAsync(cancellationToken);
            return true;
        }
    }
}
