using System.Linq;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Workflow;
using Fusion.Repository.Bases.Page.Workflowstatus;
using Fusion.Repository.Data;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Microsoft.EntityFrameworkCore;

namespace Fusion.Repository.Repositories
{
    public class WorkflowStatusRepository : GenericRepository<WorkflowStatus>,IWorkflowStatusRepository
    {
        private readonly FusionDbContext _context;

        public WorkflowStatusRepository(FusionDbContext context) : base(context) 
        {
            _context = context;
        }

        public async Task<PagedResult<WorkflowStatus>> GetWorkflowStatusesByProjectAsync(WorkflowStatusPagedRequest request)
        {
            if (request.ProjectId == null)
                return new PagedResult<WorkflowStatus>();

            var workflowId = await _context.Projects
                .Where(p => p.Id == request.ProjectId)
                .Select(p => p.WorkflowId)
                .FirstOrDefaultAsync();

            if (workflowId == null)
                return new PagedResult<WorkflowStatus>();

            var query = _context.WorkflowStatuses
                .Where(ws => ws.WorkflowId == workflowId);

            if (!string.IsNullOrWhiteSpace(request.Name))
            {
                query = query.Where(ws => ws.Name != null && ws.Name.Contains(request.Name));
            }

            return await query.OrderBy(ws => ws.Position).ToPagedResultAsync(request);
        }
        public async Task<bool> ExistsAsync(Guid statusId)
        {
            return await _context.WorkflowStatuses.AnyAsync(ws => ws.Id == statusId);
        }

    }
}
