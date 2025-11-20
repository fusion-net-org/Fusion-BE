using Fusion.Repository.ViewModels;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.ViewModels.Admin.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Fusion.API.Controllers
{
    [Route("api/dashboard")]
    [ApiController]
    //[Authorize(Roles = "Admin")]
    public class DashboardController : ControllerBase
    {
        private readonly IAdminService _adminService;
        public DashboardController(IAdminService adminService)
        {
            _adminService = adminService;
        }

        [HttpGet("totals")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<OverviewDashBoardResponse>))]
        public async Task<IActionResult> GetOverview(CancellationToken cancellationToken)
        {
            var data = await _adminService.GetTotalsAsync(cancellationToken);

            return Ok(ResponseModel<OverviewDashBoardResponse>.Ok(
                data: data,
                message: "Overview dashboard loaded successfully"));
        }

        //[HttpGet("monthly-stats")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<MonthlyStats>>))]

        //public async Task<IActionResult> GetMonthlyStats(CancellationToken cancellationToken)
        //{
        //    var result = await _adminService.GetMonthlyStatsAsync(cancellationToken);
        //    return Ok(ResponseModel<IEnumerable<MonthlyStats>>.Ok(
        //        data: result,
        //        message: "Montly Stats loaded successfully"));
        //}

        //[HttpGet("plan-rate")]
        //[ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<IEnumerable<PlanRate>>))]

        //public async Task<IActionResult> GetPlanRate(CancellationToken token)
        //{
        //    var result = await _adminService.GetTopPlanRateAsync(token);
        //    return Ok(ResponseModel<IEnumerable<PlanRate>>.Ok(
        //       data: result,
        //       message: "Plan Rate loaded successfully"));
        //}

    }
}
