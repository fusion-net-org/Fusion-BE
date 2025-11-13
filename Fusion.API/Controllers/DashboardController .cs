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
    [Authorize(Roles = "Admin")]
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


        
    }
}
