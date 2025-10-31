using Fusion.API.Auth;
using Fusion.Repository.Bases.Page;
using Fusion.Repository.Bases.Page.Company;
using Fusion.Repository.Bases.Page.User;
using Fusion.Repository.Bases.Responses;
using Fusion.Service.Commons.BaseResponses;
using Fusion.Service.IServices;
using Fusion.Service.Services;
using Fusion.Service.ViewModels.Companies.Requests;
using Fusion.Service.ViewModels.Companies.Responses;
using Fusion.Service.ViewModels.Users.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace Fusion.API.Controllers
{
    [Route("api/company")]
    [ApiController]
    public class CompanyController : ControllerBase
    {
        private readonly ICompanyService _companyService;

        public CompanyController(ICompanyService companyService)
        {
            _companyService = companyService;
        }

        [HttpGet("paged")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanyResponse>>))]
        public async Task<IActionResult> GetPaged([FromQuery] CompanyPagedSearchRequest request, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyService.GetPagedCompaniesAsync(email, request, cancellationToken);
            return Ok(ResponseModel<PagedResult<CompanyResponse>>.Ok(
                data: result,
                message: "Get paged companies successfully"));
        }

        [HttpGet("all-companies")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<PagedResult<CompanyResponse>>))]
        public async Task<IActionResult> GetAllCompanies([FromQuery] CompanyPagedSearchRequestVersion2 request, [FromQuery] Guid? companyId, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<CompanyResponseVersion2>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyService.GetAllCompaniesAsync(email, request, companyId, cancellationToken);
            return Ok(ResponseModel<PagedResult<CompanyResponseVersion2>>.Ok(
                data: result,
                message: "Get paged companies successfully"));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseModel<CompanyResponse>))]
        public async Task<IActionResult> CreateCompany(CompanyRequest request, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyService.CreateCompanyAsync(request, email, cancellationToken);

            return Ok(ResponseModel<CompanyResponse>.Ok(
                data: result,
                message: ResponseMessageHelper.FormatMessage(ResponseMessages.CREATE_SUCCESS, "company")));
        }

        [HttpGet("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyResponse>))]
        public async Task<IActionResult> GetById(Guid id, CancellationToken cancellationToken)
        {
            var result = await _companyService.GetCompanyByIdAsync(id, cancellationToken);
            return Ok(ResponseModel<CompanyResponse>.Ok(
                data: result,
                message: "Get company by id successfully"));
        }

        [HttpPut("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyResponse>))]
        public async Task<IActionResult> Update(Guid id, CompanyRequest request, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyService.UpdateCompanyAsync(id, request, email, cancellationToken);
            return Ok(ResponseModel<CompanyResponse>.Ok(
                data: result,
                message: "Update company successfully"));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            var email = emailClaim?.Value; if (email == null)
            {
                return Unauthorized(ResponseModel<CompanyResponse>.Error(
                    statusCode: StatusCodes.Status401Unauthorized,
                    message: "Unauthorized: User identity not found"
                ));
            }

            var result = await _companyService.DeleteCompanyAsync(id, email, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: result,
                message: "Delete company successfully"));
        }

        [HttpGet("{companyId:guid}/summary")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanySummaryResponse>))]
        public async Task<IActionResult> GetCompanySummary(Guid companyId)
        {
            var result = await _companyService.GetCompanySummaryAsync(companyId);
            return Ok(ResponseModel<CompanySummaryResponse>.Ok(
                 data: result,
                 message: "Fetch Company Summary successfully"));
        }

        [HttpGet("{companyId:guid}/performance")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyPerformanceResponse>))]

        public async Task<IActionResult> GetCompanyPerformance(Guid companyId)
        {
            var result = await _companyService.GetCompanyPerformanceAsync(companyId);
            return Ok(ResponseModel<CompanyPerformanceResponse>.Ok(
                 data: result,
                 message: "Fetch Company Performance successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpPut("admin/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyResponse>))]
        public async Task<IActionResult> UpdateByAdmin(Guid id, CompanyRequest request, CancellationToken cancellationToken)
        {
            var result = await _companyService.UpdateCompanyByAdminAsync(id, request, cancellationToken);
            return Ok(ResponseModel<CompanyResponse>.Ok(
                data: result,
                message: "Update company successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("admin/{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> DeleteByAdmin(Guid id, CancellationToken cancellationToken)
        {
            var result = await _companyService.DeleteCompanyByAdminAsync(id, cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: result,
                message: "Delete company successfully"));
        }

        [Authorize(Roles = "Admin")]
        [HttpDelete("getCompanyStatusCounts")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyStatusCountsVm>))]
        public async Task<IActionResult> GetCompanyStatusCounts(CancellationToken cancellationToken)
        {
            var result = await _companyService.GetCompanyStatusCountsAsync(cancellationToken);
            return Ok(ResponseModel<CompanyStatusCountsVm>.Ok(
                data: result,
                message: "Get compnay with status success."));
        }
        [Authorize(Roles = "Admin")]
        [HttpGet("stats/created-by-month")]
        [ProducesResponseType(StatusCodes.Status200OK, Type = typeof(ResponseModel<CompanyMonthlyStatsVm>))]
        public async Task<IActionResult> GetCompaniesCreatedByMonth([FromQuery] int year, CancellationToken cancellationToken)
        {
            var result = await _companyService.GetCompaniesCreatedByMonthAsync(year, cancellationToken);

            return Ok(ResponseModel<CompanyMonthlyStatsVm>.Ok(
                data: result,
                message: $"Companies created per month for {result.Year}"));
        }
    }
}
