using System.Collections.Generic;
using System.Threading.Tasks;
using AutoMapper;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Workflow;
using Fusion.Repository.Bases.Page.Workflowstatus;
using Fusion.Repository.Entities;
using Fusion.Repository.IRepositories;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.WorkflowStatus;

namespace Fusion.Service.Services
{
    public class WorkflowStatusService : IWorkflowStatusService
    {
        private readonly IWorkflowStatusRepository _repository;
        private readonly IMapper _mapper;

        public WorkflowStatusService(IWorkflowStatusRepository repository, IMapper mapper)
        {
            _repository = repository;
            _mapper = mapper;
        }

        public async Task<PagedResult<WorkflowStatusResponse>> GetWorkflowStatusesByProjectAsync(WorkflowStatusPagedRequest request)
        {
            var pagedStatuses = await _repository.GetWorkflowStatusesByProjectAsync(request);

            var mapped = _mapper.Map<List<WorkflowStatusResponse>>(pagedStatuses.Items);

            return new PagedResult<WorkflowStatusResponse>
            {
                Items = mapped,
                TotalCount = pagedStatuses.TotalCount,
                PageNumber = pagedStatuses.PageNumber,
                PageSize = pagedStatuses.PageSize
            };
        }


    }
}
