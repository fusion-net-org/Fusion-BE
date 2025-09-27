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
            var result = await _companyService.GetPagedCompaniesAsync(request, cancellationToken);
            return Ok(ResponseModel<PagedResult<CompanyResponse>>.Ok(
                data: result,
                message: "Get paged companies successfully"));
        }

        [HttpPost]
        [ProducesResponseType(StatusCodes.Status201Created, Type = typeof(ResponseModel<CompanyResponse>))]
        public async Task<IActionResult> CreateCompany(CompanyRequest request, CancellationToken cancellationToken)
        {
            //var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            //var email = emailClaim?.Value; if (email == null)
            //{
            //    return Unauthorized(ResponseModel<CompanyResponse>.Error(
            //        statusCode: StatusCodes.Status401Unauthorized,
            //        message: "Unauthorized: User identity not found"
            //    ));
            //}

            var result = await _companyService.CreateCompanyAsync(request, "minh04122003@gmail.com", cancellationToken);

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
            //var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            //var email = emailClaim?.Value; if (email == null)
            //{
            //    return Unauthorized(ResponseModel<CompanyResponse>.Error(
            //        statusCode: StatusCodes.Status401Unauthorized,
            //        message: "Unauthorized: User identity not found"
            //    ));
            //}

            var result = await _companyService.UpdateCompanyAsync(id, request, "minh04122003@gmail.com", cancellationToken);
            return Ok(ResponseModel<CompanyResponse>.Ok(
                data: result,
                message: "Update company successfully"));
        }

        [HttpDelete("{id:guid}")]
        [ProducesResponseType(StatusCodes.Status204NoContent)]
        public async Task<IActionResult> Delete(Guid id, CancellationToken cancellationToken)
        {
            //var emailClaim = User.Claims.FirstOrDefault(c => c.Type == JwtRegisteredClaimNames.Email || c.Type == ClaimTypes.Email || c.Type == "email");
            //var email = emailClaim?.Value; if (email == null)
            //{
            //    return Unauthorized(ResponseModel<CompanyResponse>.Error(
            //        statusCode: StatusCodes.Status401Unauthorized,
            //        message: "Unauthorized: User identity not found"
            //    ));
            //}

            var result = await _companyService.DeleteCompanyAsync(id, "minh04122003@gmail.com", cancellationToken);
            return Ok(ResponseModel<bool>.Ok(
                data: result,
                message: "Delete company successfully"));
        }
    }
}
