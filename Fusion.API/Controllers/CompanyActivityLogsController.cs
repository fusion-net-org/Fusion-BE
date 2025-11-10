using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.CompanyActivityLog;
using Fusion.Repository.Entities;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.CompanyActivityLog.Requests;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;


namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanyActivityLogsController : ControllerBase
    {
        private readonly ICompanyActivityService _service;

        public CompanyActivityLogsController(ICompanyActivityService service)
        {
            _service = service;
        }
        [HttpGet]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanyActivityLog>>))]
        [ProducesResponseType(StatusCodes.Status401Unauthorized, Type = typeof(ResponseModel<PagedResult<CompanyActivityLog>>))]
        public async Task<IActionResult> GetPaged( Guid companyId, [FromQuery] CompanyActivityLogQuery? query, CancellationToken cancellationToken)
        {
            var req = new CompanyActivityLogPagedSearchRequest
            {
                KeyWord = query?.Keyword,
                DateRange = (query?.From.HasValue == true || query?.To.HasValue == true)
            ? new DateRangeRequest { From = query!.From, To = query!.To }
            : null,

                PageNumber = query?.PageNumber ?? 1,
                PageSize = query?.PageSize ?? 10,
                SortColumn = string.IsNullOrWhiteSpace(query?.SortColumn) ? "CreatedAt" : query!.SortColumn,
                SortDescending = query?.SortDescending ?? true
            };

            var result = await _service.GetPagedAsync(companyId, req, cancellationToken);
            return Ok(ResponseModel<PagedResult<CompanyActivityLog>>.Ok(result, "Fetched activity logs successfully"));
        }
        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyActivityLog>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<CompanyActivityLog>))]
        public async Task<IActionResult> GetById(
           [FromRoute] Guid companyId,
           [FromRoute] Guid id,
           CancellationToken cancellationToken)
        {
            var log = await _service.GetByIdAsync(id, cancellationToken);
              
            return Ok(ResponseModel<CompanyActivityLog>.Ok(log, "Fetched activity log successfully"));
        }

        [HttpPut("update_isView")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> SetVisibilityForOne(
           bool isView,
           Guid companyId,
           CancellationToken ct)
        {
            var updated = await _service.UpdateIsView(isView, companyId, ct);
            return Ok(ResponseModel<bool>.Ok(updated, "Visibility updated."));
        }

        [HttpPost("request_viewLog")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<bool>))]
        [ProducesResponseType(StatusCodes.Status404NotFound, Type = typeof(ResponseModel<bool>))]
        public async Task<IActionResult> RequestViewLog(
           Guid companyIdA,
           Guid companyIdB,
           CancellationToken ct)
        {
            var request = await _service.RequestViewLog(companyIdA, companyIdB, ct);
            return Ok(ResponseModel<bool>.Ok(request, "Request success."));
        }


    }
}
