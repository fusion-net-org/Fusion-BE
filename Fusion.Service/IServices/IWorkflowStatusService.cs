using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Workflowstatus;
using Fusion.Repository.Entities;
using Fusion.Service.ViewModels.WorkflowStatus;

namespace Fusion.Service.IServices
{
    public interface IWorkflowStatusService
    {
        Task<PagedResult<WorkflowStatusResponse>> GetWorkflowStatusesByProjectAsync(WorkflowStatusPagedRequest request);
    }
}
