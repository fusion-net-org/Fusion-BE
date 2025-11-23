
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Admin.Responses;
using Fusion.Service.ViewModels.TransactionPayment.Responses.Overview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Fusion.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = "Admin")]
    public class DashBoardController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public DashBoardController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("total_entities")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<OverviewDashBoardResponse>))]
        public async Task<IActionResult> GetTotalsAsync(CancellationToken ct = default)
        {
            var result = await _adminService.GetTotalsAsync(ct);
            return Ok(ResponseModel<OverviewDashBoardResponse>.Ok(
                    data: result,
                    message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Get total entities successfully")));
        }
        [HttpGet("purchase-ratio")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<IReadOnlyList<PlanPurchaseRatioItemResponse>>))]
        public async Task<IActionResult> GetPlanPurchaseRatio(CancellationToken ct = default)
        {
            var data = await _adminService.GetPlanPurchaseRatioAsync(ct);
            return Ok(ResponseModel<IReadOnlyList<PlanPurchaseRatioItemResponse>>.Ok(
                    data: data,
                    message: ResponseMessageHelper.FormatMessage(ResponseMessages.GET_SUCCESS, "Get plan purchase ratio successfully")));
        }

    }
}
