using Fusion.Repository.Bases.Responses;
using Fusion.Repository.Repositories;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.Services;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [ApiController]
    [Route("api")]
    public class WorkflowsController : ControllerBase
    {
        private readonly IWorkflowDesignerService _svc;
        public WorkflowsController(IWorkflowDesignerService svc) => _svc = svc;

        // GET /api/companies/{companyId}/workflows
        [HttpGet("companies/{companyId:guid}/workflows")]
        public async Task<IActionResult> List(Guid companyId, CancellationToken ct)
        {
            var items = await _svc.GetAllAsync(companyId, ct);
            return Ok(ResponseModel<List<WorkflowListItemVm>>.Ok(
                data: items,
                message: "Get workflows successfully"));
        }

        // POST /api/companies/{companyId}/workflows
        public record CreateWorkflowReq(string Name);
        [HttpPost("companies/{companyId:guid}/workflows")]
        [Consumes("application/json")]
        public async Task<IActionResult> Create(Guid companyId, [FromBody] CreateWorkflowReq req, CancellationToken ct)
        {
            var id = await _svc.CreateAsync(companyId, req.Name, ct);
            return CreatedAtAction(nameof(GetDesigner), new { workflowId = id },
                ResponseModel<object>.Ok(new { id },
                    ResponseMessageHelper.FormatMessage(ResponseMessages.SAVE_SUCCESS, "Tạo workflow thành công")));
        }

        // GET /api/workflows/{workflowId}/designer
        [HttpGet("workflows/{workflowId:guid}/designer")]
        public async Task<IActionResult> GetDesigner(Guid workflowId, CancellationToken ct)
        {
            var dto = await _svc.GetDesignerAsync(workflowId, ct);
            return Ok(ResponseModel<DesignerDto>.Ok(dto, "Get designer successfully"));
        }

        // PUT /api/workflows/{workflowId}/designer?companyId=...
        [HttpPut("workflows/{workflowId:guid}/designer")]
        [Consumes("application/json")]
        public async Task<IActionResult> SaveDesigner(Guid workflowId, [FromQuery] Guid companyId, [FromBody] DesignerDto dto, CancellationToken ct)
        {
            await _svc.SaveDesignerAsync(companyId, workflowId, dto, ct);
            return Ok(ResponseModel<object>.Ok(new { workflowId },
                ResponseMessageHelper.FormatMessage(ResponseMessages.SAVE_SUCCESS, "Lưu workflow thành công")));
        }

        // DELETE /api/companies/{companyId}/workflows/{workflowId}
        [HttpDelete("companies/{companyId:guid}/workflows/{workflowId:guid}")]
        public async Task<IActionResult> Delete(Guid companyId, Guid workflowId, CancellationToken ct)
        {
            await _svc.DeleteAsync(companyId, workflowId, ct);
            return Ok(ResponseModel<object>.Ok(new { workflowId },
                ResponseMessageHelper.FormatMessage(ResponseMessages.DELETE_SUCCESS, "Xoá workflow thành công")));
        }
    }
}
