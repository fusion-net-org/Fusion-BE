using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Workflowstatus;
using Fusion.Repository.Entities;

namespace Fusion.Repository.IRepositories
{
    public interface IWorkflowStatusRepository
    {
        Task<PagedResult<WorkflowStatus>> GetWorkflowStatusesByProjectAsync(WorkflowStatusPagedRequest request);
    }
}
