

using Fusion.Repository.Bases.Exceptions;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class ProjectRepository : GenericRepository<Project>, IProjectRepository
    {
        private readonly FusionDbContext _context;
        public ProjectRepository(FusionDbContext context) : base(context)
        {
            _context = context;
        }

        public async Task<Project> CreateProjectAsync(Guid userId, Project request, CancellationToken cancellationToken = default)
        {
            var now = DateTime.UtcNow;
            request.CreateAt = now;
            request.UpdateAt = now;
            request.CreatedBy = userId;

            await _context.Projects.AddAsync(request, cancellationToken);
            try
            {
                await _context.SaveChangesAsync(cancellationToken);
                return request;
            }
            catch (DbUpdateException ex)
            {
                var detail = ex.InnerException?.Message ?? ex.Message;
                throw;
            }
        }
    }
}
